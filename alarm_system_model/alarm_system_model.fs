namespace alarm_system_model

open alarm_system_common

type AlarmSystemModel(switchToArmedTime, switchToFlashTime, switchToSilentAndOpenTime) = 

    let switchToArmedTime = switchToArmedTime
    let switchToFlashTime = switchToFlashTime
    let switchToSilentAndOpenTime = switchToSilentAndOpenTime
    let mutable currentState = AlarmSystemStateType.OpenAndUnlocked

    let stateChanged = new DelegateEvent<System.EventHandler<StateChangedEventArgs>>()

    member this.asyncSwitchToState fromState switchTime toState = async {
            do! Async.Sleep(switchTime) 
            if fromState = currentState then 
                this.setState toState
        }

    member this.asyncSwitchTimedToArmed fromState =
        Async.CancelDefaultToken()
        ignore (Async.Start ((this.asyncSwitchToState fromState switchToArmedTime AlarmSystemStateType.Armed), Async.DefaultCancellationToken))

    member this.asyncSwitchTimedToFlash fromState =
        Async.CancelDefaultToken()
        ignore (Async.Start ((this.asyncSwitchToState fromState switchToFlashTime AlarmSystemStateType.AlarmFlash), Async.DefaultCancellationToken))

    member this.asyncSwitchTimedToSilentAndOpen fromState =
        Async.CancelDefaultToken()
        ignore (Async.Start ((this.asyncSwitchToState fromState switchToSilentAndOpenTime AlarmSystemStateType.SilentAndOpen), Async.DefaultCancellationToken))

    override this.ToString() = currentState.ToString()

    member this.setState newState =
        Async.CancelDefaultToken()
        let oldState = currentState
        currentState <- newState

        match currentState with
        | AlarmSystemStateType.ClosedAndLocked -> this.asyncSwitchTimedToArmed currentState
        | AlarmSystemStateType.AlarmFlashAndSound -> this.asyncSwitchTimedToFlash currentState
        | AlarmSystemStateType.AlarmFlash -> this.asyncSwitchTimedToSilentAndOpen currentState
        | _ -> ()

        this.FireEvent(oldState, newState)

    member this.FireEvent(oldState, newState) =
        stateChanged.Trigger([|this; new StateChangedEventArgs(oldState, newState)|])

    member this.MyReadWriteProperty with get () = 5

    interface AlarmSystem with

        member this.Open() = 
            match currentState with
            | AlarmSystemStateType.ClosedAndUnlocked -> this.setState AlarmSystemStateType.OpenAndUnlocked
            | AlarmSystemStateType.ClosedAndLocked -> this.setState AlarmSystemStateType.OpenAndLocked
            | AlarmSystemStateType.Armed -> this.setState AlarmSystemStateType.AlarmFlashAndSound
            | _ -> ()

        member this.Close() = 
            match currentState with
            | AlarmSystemStateType.OpenAndUnlocked -> this.setState AlarmSystemStateType.ClosedAndUnlocked
            | AlarmSystemStateType.OpenAndLocked -> this.setState AlarmSystemStateType.ClosedAndLocked
            | AlarmSystemStateType.SilentAndOpen -> this.setState AlarmSystemStateType.Armed
            | _ -> ()

        member this.Lock() =
            match currentState with
            | AlarmSystemStateType.OpenAndUnlocked -> this.setState AlarmSystemStateType.OpenAndLocked
            | AlarmSystemStateType.ClosedAndUnlocked -> this.setState AlarmSystemStateType.ClosedAndLocked
            | _ -> ()
    
        member this.Unlock() =
            match currentState with
            | AlarmSystemStateType.OpenAndLocked -> this.setState AlarmSystemStateType.OpenAndUnlocked
            | AlarmSystemStateType.ClosedAndLocked -> this.setState AlarmSystemStateType.ClosedAndUnlocked
            | AlarmSystemStateType.Armed -> this.setState AlarmSystemStateType.ClosedAndUnlocked
            | AlarmSystemStateType.AlarmFlashAndSound -> this.setState AlarmSystemStateType.OpenAndUnlocked
            | AlarmSystemStateType.AlarmFlash -> this.setState AlarmSystemStateType.OpenAndUnlocked
            | AlarmSystemStateType.SilentAndOpen -> this.setState AlarmSystemStateType.OpenAndUnlocked
            | _ -> ()

        member this.ShutDown() = 
            Async.CancelDefaultToken()

        [<CLIEvent>]
        member this.StateChanged = stateChanged.Publish

        member this.CurrentStateType with get () = currentState