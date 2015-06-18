module fscheck_alarm_system_test

// https://github.com/fsharp/FsCheck/blob/master/Docs/Documentation.md
// https://github.com/fsharp/FsUnit
// https://code.google.com/p/unquote/

open FsCheck

open alarm_system_common
open alarm_system
open alarm_system_model

open System
open System.Threading
open System.Collections.Generic
open System.Threading.Tasks

//type AlarmSystemState = OpenAndUnlocked | ClosedAndUnlocked | OpenAndLocked
//                        | ClosedAndLocked | Armed | SilentAndOpen | AlarmFlashAndSound | AlarmFlash

let switchToArmedTime = 20
let switchToFlashTime = 40
let switchToSilentAndOpenTime = 100
let allowedWrongPinCodeCount = 3
let allowedWrongSetPinCodeCount = 3

let DELTA_COMPARE = TimeSpan.FromMilliseconds(2.)
let DELTA_WAIT = 5

[<Struct>]
type StateChangedEvent =
    val State: StateChangedEventArgs
    val Time: DateTime
    new(state, time) = {State = state; Time = time}

type WaitResult =
    { isOK : bool; 
      Waited : bool }

type StateChangedEvents =
    { Current : StateChangedEvent;
      Before : StateChangedEvent }

type MIStateChangedEvents =
    { Model : StateChangedEvents;
      Impl : StateChangedEvents}

// ----------------------------------------------------------------------------------

let monitor = new Object()

//let modelEventWait = new AutoResetEvent(false)
//let implEventWait = new AutoResetEvent(false)

let implEventList = new Stack<StateChangedEvent>()
let modelEventList = new Stack<StateChangedEvent>()

let implementationStateChangedEventHandler (args : StateChangedEventArgs) = 
    let now = DateTime.Now
    do (now |> ignore)
    
    //printfn "impleventhandler: new: %s | old: %s at %d " (args.NewStateType.ToString()) (args.OldStateType.ToString()) now.Ticks
    lock monitor
        (fun () -> implEventList.Push(new StateChangedEvent(args, now)); Monitor.Pulse(monitor) |> ignore )

let modelStateChangedEventHandler (args : StateChangedEventArgs) = 
    let now = DateTime.Now
    do (now |> ignore)

    //printfn "modeleventhandler: new: %s | old: %s at %d " (args.NewStateType.ToString()) (args.OldStateType.ToString()) now.Ticks
    
    lock monitor
        (fun () -> modelEventList.Push(new StateChangedEvent(args, now)); Monitor.Pulse(monitor) |> ignore )

let popImplStateChangedEvents() =
    lock implEventList 
        (fun () -> { Current = implEventList.Peek(); Before = implEventList.Peek() } )

let popModelStateChangedEvents() =
    lock modelEventList 
        (fun () -> { Current = modelEventList.Peek(); Before = modelEventList.Peek() } )

let clearAllStateChangedEvents() =
    lock monitor (fun () -> modelEventList.Clear(); implEventList.Clear())        

// ----------------------------------------------------------------------------------

let implMessageList = new Stack<string>()
let modelMessageList = new Stack<string>()

let modelMessageEventHandler (message : string) =
    lock modelMessageList
        (fun () -> modelMessageList.Push(message))

let implMessageEventHandler (message : string) =
    lock implMessageList
        (fun () -> implMessageList.Push(message))

let empty = "<empty>"

let popModelMessage() =
    lock modelMessageList 
        (fun () -> if modelMessageList.Count = 0 then empty else modelMessageList.Pop())

let popImplMessage() =
    lock implMessageList 
        (fun () -> if implMessageList.Count = 0 then empty else implMessageList.Pop())

let clearMessages() =
    lock implMessageList 
        (fun () -> implMessageList.Clear())
    lock modelMessageList 
        (fun () -> modelMessageList.Clear())

// ----------------------------------------------------------------------------------

let compareTime (time1 : TimeSpan) (time2 : TimeSpan) (modelEvents :StateChangedEvents) (implEvents :StateChangedEvents) =
    let diff = (time1 - time2)

    if -DELTA_COMPARE <= diff && diff <= DELTA_COMPARE then 
        true
    else 
        printfn "The time needed for the state switch was too big: %f | %s | %s" diff.TotalMilliseconds (modelEvents.Current.State.NewStateType.ToString()) (implEvents.Current.State.NewStateType.ToString())
        false

// ----------------------------------------------------------------------------------

let waitAndCheckTimedEvent (model : AlarmSystem) (impl : AlarmSystem) (timeToWait : int) fromState = 
    System.Threading.Thread.Sleep(timeToWait)
    
//    let wait = async {
//        WaitHandle.WaitAll([|modelEventWait :> WaitHandle; implEventWait :> WaitHandle|], timeToWait) |> ignore}
//
//    let task = Async.StartAsTask(wait, TaskCreationOptions.None, new CancellationToken())
//    task.Wait()

//    WaitHandle.WaitAny([|modelEventWait :> WaitHandle|], timeToWait + DELTA_WAIT) |> ignore
//    WaitHandle.WaitAny([|implEventWait :> WaitHandle |], DELTA_WAIT) |> ignore

//    let implEvents = popImplStateChangedEvents()
//    let modelEvents = popModelStateChangedEvents()

    let r = lock monitor
                (fun () -> 
                    let mutable tries = 5
                    //printfn "init: m: %d | i: %d" (modelEventList.Count) (implEventList.Count)
                    while implEventList.Count <> modelEventList.Count && tries > 0 do
                        Monitor.Wait(monitor, DELTA_WAIT) |> ignore
                        tries <- tries - 1

                    if implEventList.Count <> modelEventList.Count then
                        printfn "------->>>> we have a different count of events, there must someting wrong with the timing!!!"

                    {Model = popModelStateChangedEvents(); Impl = popImplStateChangedEvents()})

    let implEvents = r.Impl
    let modelEvents = r.Model
   
    let isTimeDiffOk = compareTime (modelEvents.Current.Time - modelEvents.Before.Time) 
                                   (implEvents.Current.Time - implEvents.Before.Time) modelEvents implEvents

    if modelEvents.Before.State.NewStateType = implEvents.Before.State.NewStateType
        && modelEvents.Current.State.NewStateType = implEvents.Current.State.NewStateType
        && isTimeDiffOk then
        {WaitResult.isOK = true; WaitResult.Waited = true}
    else
        {WaitResult.isOK = false; WaitResult.Waited = true}


let checkForTimedStateChange (model : AlarmSystem) (impl : AlarmSystem) wait = 

    let currentState = model.CurrentState

    if not wait then
        {WaitResult.isOK = (model.CurrentState = impl.CurrentState); WaitResult.Waited = false}

    else
        
        let timeToWait =
            match currentState with
            | AlarmSystemState.ClosedAndLocked -> Some switchToArmedTime
            | AlarmSystemState.AlarmFlashAndSound -> Some switchToFlashTime
            | AlarmSystemState.AlarmFlash -> Some switchToSilentAndOpenTime
            | _ -> None

        if timeToWait.IsSome then
            waitAndCheckTimedEvent model impl timeToWait.Value currentState
        else
            {WaitResult.isOK = (model.CurrentState = impl.CurrentState); WaitResult.Waited = false}

// ----------------------------------------------------------------------------------

let pinGenerator = Gen.oneof [ Gen.choose (0,9999); gen { return 1234 }]
let getRandomPin seed = (Gen.sample 0 seed (pinGenerator)).Head.ToString()

let waitGenerator = Gen.oneof [ gen { return true }; gen { return false }]
let getRandomShouldWait seed = (waitGenerator |> Gen.sample 0 seed).Head

let printWaitedIfNeed text wait = 
    if wait then
        text + "-w"
    else
        text
        
let getDebugString called waited = 
    (printWaitedIfNeed called waited) //+ "[" + model.ToString() + "|" + impl.ToString() + "]"

let getDebugStringWithPin called waited pin =
    (printWaitedIfNeed called waited) + "(" + pin + ")" //+ "[" + model.ToString() + "|" + impl.ToString() + "]"

// ----------------------------------------------------------------------------------
let spec =

    let specOpen wait =
        let waited = ref false;
        let model = ref -1
        let impl = ref -1
      
        { new Command<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Open(); c
            member x.RunModel m = m.Open(); m
            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c wait
                waited := timedCheck.Waited

                timedCheck.isOK |> Prop.ofTestable

            override x.ToString() = getDebugString "open" !waited}
            

    let specClose wait = 
        let waited = ref false;
      
        { new Command<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Close(); c
            member x.RunModel m = m.Close(); m
            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c wait
                waited := timedCheck.Waited

                timedCheck.isOK |> Prop.ofTestable
            
            override x.ToString() = getDebugString "close" !waited}

    let specLock wait = 
        let waited = ref false;
      
        { new Command<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Lock(); c
            member x.RunModel m = m.Lock(); m
            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c wait
                waited := timedCheck.Waited

                timedCheck.isOK |> Prop.ofTestable
            
            override x.ToString() = getDebugString "lock" !waited }

    let specUnlock wait pinCode = 
        let waited = ref false;

        { new Command<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.Unlock(pinCode); c
            member x.RunModel m = m.Unlock(pinCode); m
            member x.Post (c,m) = 
                let timedCheck = checkForTimedStateChange m c wait
                waited := timedCheck.Waited

                timedCheck.isOK |> Prop.ofTestable
            
            override x.ToString() = getDebugStringWithPin "unlock" !waited pinCode}

    let specSetPinCode pinCode newPinCode = 
        
        { new Command<AlarmSystem, AlarmSystem>() with
            member x.RunActual c = c.SetPinCode(pinCode, newPinCode); c
            member x.RunModel m = m.SetPinCode(pinCode, newPinCode); m
            member x.Post (c,m) = 
                let modelMessage = popModelMessage()
                let implMessage = popImplMessage()
                clearMessages()
                
                (modelMessage = implMessage && m.CurrentState = c.CurrentState) |> Prop.ofTestable

            override x.ToString() = String.Concat [|"setPinCode("; pinCode; ","; newPinCode; ")" |] }


    { new ICommandGenerator<AlarmSystem,AlarmSystem> with
        member __.InitialActual = 
            let alarmSystemImpl = new AlarmSystemImpl(switchToArmedTime, 30, 
                                                        switchToSilentAndOpenTime, allowedWrongPinCodeCount,
                                                        allowedWrongSetPinCodeCount) :> AlarmSystem

            clearAllStateChangedEvents()

            alarmSystemImpl.StateChanged.Add(implementationStateChangedEventHandler)
            alarmSystemImpl.MessageArrived.Add(implMessageEventHandler)
            
            alarmSystemImpl

        member __.InitialModel = 
            let alarmSystemModel = new AlarmSystemModel(switchToArmedTime, switchToFlashTime, 
                                                    switchToSilentAndOpenTime, allowedWrongPinCodeCount, 
                                                    allowedWrongSetPinCodeCount) :> AlarmSystem

            clearAllStateChangedEvents()

            alarmSystemModel.StateChanged.Add(modelStateChangedEventHandler)
            alarmSystemModel.MessageArrived.Add(modelMessageEventHandler) 
            alarmSystemModel  

        member __.Next model = Gen.oneof [ gen {return getRandomShouldWait(2) |> specOpen};
                                            gen {return getRandomShouldWait(3) |> specClose};
                                            gen {return getRandomShouldWait(4) |> specLock};
                                            gen {return getRandomPin(8) |> specUnlock (getRandomShouldWait(5))};
                                            gen {return getRandomPin(9) |> specSetPinCode (getRandomPin(6))} ] }


let config = {
    Config.Quick with 
        MaxTest = 100
    }

//let config = {
//    Config.Quick with 
//        Replay = Random.StdGen (154938806,296021545) |> Some 
//    }

AlarmSystemImpl.ShutDownAll()
//Check.Verbose(asProperty spec)
Check.One(config, (Command.toProperty spec))
//Check.One (config, (asProperty spec))
AlarmSystemImpl.ShutDownAll()


