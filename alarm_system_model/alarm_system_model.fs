namespace alarm_system_model

open alarm_system_common

type AlarmSystemModel(switchToArmedTime, switchToFlashTime, switchToSilentAndOpenTime) = 

    let switchToArmedTime = switchToArmedTime
    let switchToFlashTime = switchToFlashTime
    let switchToSilentAndOpenTime = switchToSilentAndOpenTime
    let mutable currentState = AlarmSystemState.OpenAndUnlocked

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

    member this.FireStateChangedEvent(oldState, newState) =
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
            | AlarmSystemState.OpenAndLocked -> this.setState AlarmSystemState.OpenAndUnlocked
            | AlarmSystemState.ClosedAndLocked -> this.setState AlarmSystemState.ClosedAndUnlocked
            | AlarmSystemState.Armed -> this.setState AlarmSystemState.ClosedAndUnlocked
            | AlarmSystemState.AlarmFlashAndSound -> this.setState AlarmSystemState.OpenAndUnlocked
            | AlarmSystemState.AlarmFlash -> this.setState AlarmSystemState.OpenAndUnlocked
            | AlarmSystemState.SilentAndOpen -> this.setState AlarmSystemState.OpenAndUnlocked
            | _ -> ()

        member this.ShutDown() = 
            Async.CancelDefaultToken()

        [<CLIEvent>]
        member this.StateChanged = stateChanged.Publish

        member this.CurrentState with get () = currentState