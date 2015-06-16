namespace alarm_system_model

open alarm_system_common

type AlarmSystemModel(switchToArmedTime, switchToFlashTime, switchToSilentAndOpenTime, allowedWrongPinCodeCount) = 

    let switchToArmedTime = switchToArmedTime
    let switchToFlashTime = switchToFlashTime
    let switchToSilentAndOpenTime = switchToSilentAndOpenTime
    let mutable currentState = AlarmSystemState.OpenAndUnlocked

    let ALLOWED_WRONG_PIN_CODE_COUNT = allowedWrongPinCodeCount

    let mutable alarmSystemPinCode = "1234"
    let mutable wrongPinCodeCounter = 0;

    let stateChanged = new DelegateEvent<System.EventHandler<StateChangedEventArgs>>()

    member this.asyncSwitchToState fromState switchTime toState = async {
            do! Async.Sleep(switchTime) 
            if fromState = currentState then 
                this.setState toState
        }

    member this.asyncSwitchTimedToArmed fromState =
        Async.CancelDefaultToken()
        ignore (Async.Start ((this.asyncSwitchToState fromState switchToArmedTime AlarmSystemState.Armed), Async.DefaultCancellationToken))

    member this.asyncSwitchTimedToFlash fromState =
        Async.CancelDefaultToken()
        ignore (Async.Start ((this.asyncSwitchToState fromState switchToFlashTime AlarmSystemState.AlarmFlash), Async.DefaultCancellationToken))

    member this.asyncSwitchTimedToSilentAndOpen fromState =
        Async.CancelDefaultToken()
        ignore (Async.Start ((this.asyncSwitchToState fromState switchToSilentAndOpenTime AlarmSystemState.SilentAndOpen), Async.DefaultCancellationToken))

    override this.ToString() = currentState.ToString()

    member this.setState newState =
        Async.CancelDefaultToken()
        let oldState = currentState
        currentState <- newState

        match currentState with
        | AlarmSystemState.ClosedAndLocked -> this.asyncSwitchTimedToArmed currentState
        | AlarmSystemState.AlarmFlashAndSound -> this.asyncSwitchTimedToFlash currentState
        | AlarmSystemState.AlarmFlash -> this.asyncSwitchTimedToSilentAndOpen currentState
        | _ -> ()

        this.FireStateChangedEvent(oldState, newState)

    member this.setStateWithPin newState pin =
        if pin = alarmSystemPinCode then
            this.setState newState

    member this.FireStateChangedEvent(oldState, newState) =
        if oldState <> newState then
            stateChanged.Trigger([|this; new StateChangedEventArgs(oldState, newState)|])

    interface AlarmSystem with

        member this.Open() = 
            match currentState with
            | AlarmSystemState.ClosedAndUnlocked -> this.setState AlarmSystemState.OpenAndUnlocked
            | AlarmSystemState.ClosedAndLocked -> this.setState AlarmSystemState.OpenAndLocked
            | AlarmSystemState.Armed -> this.setState AlarmSystemState.AlarmFlashAndSound
            | _ -> ()

        member this.Close() = 
            match currentState with
            | AlarmSystemState.OpenAndUnlocked -> this.setState AlarmSystemState.ClosedAndUnlocked
            | AlarmSystemState.OpenAndLocked -> this.setState AlarmSystemState.ClosedAndLocked
            | AlarmSystemState.SilentAndOpen -> this.setState AlarmSystemState.Armed
            | _ -> ()

        member this.Lock() =
            match currentState with
            | AlarmSystemState.OpenAndUnlocked -> this.setState AlarmSystemState.OpenAndLocked
            | AlarmSystemState.ClosedAndUnlocked -> this.setState AlarmSystemState.ClosedAndLocked
            | _ -> ()
    
        member this.Unlock(pinCode) =
            match currentState with
            | AlarmSystemState.OpenAndLocked -> this.setStateWithPin AlarmSystemState.OpenAndUnlocked pinCode
            | AlarmSystemState.ClosedAndLocked -> this.setStateWithPin AlarmSystemState.ClosedAndUnlocked pinCode
            | AlarmSystemState.Armed -> 
                if pinCode = alarmSystemPinCode then
                    wrongPinCodeCounter <- 0
                    this.setState AlarmSystemState.ClosedAndUnlocked
                else
                    wrongPinCodeCounter <- wrongPinCodeCounter + 1
                    if wrongPinCodeCounter >= ALLOWED_WRONG_PIN_CODE_COUNT then
                        this.setState AlarmSystemState.AlarmFlashAndSound

            | AlarmSystemState.AlarmFlashAndSound -> this.setStateWithPin AlarmSystemState.OpenAndUnlocked pinCode
            | AlarmSystemState.AlarmFlash -> this.setStateWithPin AlarmSystemState.OpenAndUnlocked pinCode
            | AlarmSystemState.SilentAndOpen -> this.setStateWithPin AlarmSystemState.OpenAndUnlocked pinCode
            | _ -> ()

        member this.ShutDown() = 
            Async.CancelDefaultToken()

        [<CLIEvent>]
        member this.StateChanged = stateChanged.Publish

        member this.CurrentState with get () = currentState