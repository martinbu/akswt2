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

let switchToArmedTime = 40
let switchToFlashTime = 80
let switchToSilentAndOpenTime = 120
let allowedWrongPinCodeCount = 3
let allowedWrongSetPinCodeCount = 3

let DELTA_COMPARE = TimeSpan.FromMilliseconds(15.)
let DELTA_WAIT = 10

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

let implEventList = new List<StateChangedEvent>()
let modelEventList = new List<StateChangedEvent>()

let implementationStateChangedEventHandler (args : StateChangedEventArgs) = 
    let now = DateTime.Now
    lock monitor
        (fun () -> implEventList.Add(new StateChangedEvent(args, now)); Monitor.Pulse(monitor) |> ignore )

let modelStateChangedEventHandler (args : StateChangedEventArgs) = 
    let now = DateTime.Now
    lock monitor
        (fun () -> modelEventList.Add(new StateChangedEvent(args, now)); Monitor.Pulse(monitor) |> ignore )

let getLastImplStateChangedEvents() =
    if implEventList.Count < 2 then
        { Current = implEventList.Item(implEventList.Count - 1); Before = implEventList.Item(implEventList.Count - 1) }
    else
        { Current = implEventList.Item(implEventList.Count - 1); Before = implEventList.Item(implEventList.Count - 2) }

let getLastModelStateChangedEvents() =
    if modelEventList.Count < 2 then
        { Current = modelEventList.Item(implEventList.Count - 1); Before = modelEventList.Item(implEventList.Count - 1) } 
    else
        { Current = modelEventList.Item(implEventList.Count - 1); Before = modelEventList.Item(implEventList.Count - 2) }

let clearModelEvents() = 
    lock monitor (fun () -> modelEventList.Clear())

let clearImplEvents() =
    lock monitor (fun () -> implEventList.Clear())        

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

let clearImplMessages() =
    lock implMessageList 
        (fun () -> implMessageList.Clear())

let clearModelMessages() =
    lock modelMessageList 
        (fun () -> modelMessageList.Clear())

// ----------------------------------------------------------------------------------

let compareTime (time1 : TimeSpan) (time2 : TimeSpan) (modelEvents :StateChangedEvents) (implEvents :StateChangedEvents) =
    let diff = (time1 - time2)
    //printfn "time difference: %f" diff

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
                    let mutable tries = 10
                    //printfn "init: m: %d | i: %d" (modelEventList.Count) (implEventList.Count)
                    while implEventList.Count <> modelEventList.Count && tries > 0 do
                        Monitor.Wait(monitor, DELTA_WAIT) |> ignore
                        tries <- tries - 1

                    if implEventList.Count <> modelEventList.Count then
                        printfn "------->>>> we have a different count of events, 
                            there must someting wrong with the timing!!! impl events: %d, model events: %d" 
                            implEventList.Count modelEventList.Count
                        None
                    else
                        Some {Model = getLastModelStateChangedEvents(); Impl = getLastImplStateChangedEvents()})

    if (r.IsNone) then
        {WaitResult.isOK = false; WaitResult.Waited = true}
    else
        let implEvents = r.Value.Impl
        let modelEvents = r.Value.Model

        //printfn "times: %d %d %d %d" modelEvents.Current.Time.Ticks modelEvents.Before.Time.Ticks implEvents.Current.Time.Ticks implEvents.Before.Time.Ticks
   
        let isTimeDiffOk = compareTime (modelEvents.Current.Time - modelEvents.Before.Time) 
                                       (implEvents.Current.Time - implEvents.Before.Time) modelEvents implEvents

        if modelEvents.Before.State.NewStateType = implEvents.Before.State.NewStateType
            && modelEvents.Current.State.NewStateType = implEvents.Current.State.NewStateType
            && isTimeDiffOk then
            {WaitResult.isOK = true; WaitResult.Waited = true}
        else
            printfn "wrong state or time after wait before model: %s, before impl: %s, new model: %s, new impl: %s" 
                (modelEvents.Before.State.NewStateType.ToString()) (implEvents.Before.State.NewStateType.ToString())
                (modelEvents.Current.State.NewStateType.ToString()) (implEvents.Current.State.NewStateType.ToString())
            {WaitResult.isOK = false; WaitResult.Waited = true}


let checkForTimedStateChange (model : AlarmSystem) (impl : AlarmSystem) wait = 

    let currentState = model.CurrentState

    if not wait then
        let result = (model.CurrentState = impl.CurrentState)
        if not result then
            printfn "wrong state in not wait m: %s, i: %s" (model.CurrentState.ToString()) (impl.CurrentState.ToString())
        {WaitResult.isOK = result; WaitResult.Waited = false}

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
            let result = (model.CurrentState = impl.CurrentState)
            if not result then
                printfn "wrong state m: %s, i: %s" (model.CurrentState.ToString()) (impl.CurrentState.ToString())
            {WaitResult.isOK = result; WaitResult.Waited = false}

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
                let result = m.CurrentState = c.CurrentState
                let modelMessage = popModelMessage()
                let implMessage = popImplMessage()
                clearModelMessages()
                clearImplMessages()

                if (modelMessage <> implMessage) then
                    printfn "message not equal! %s %s" modelMessage implMessage

                if not result then
                    printfn "wrong state m: %s, i: %s" (m.CurrentState.ToString()) (c.CurrentState.ToString())
                (modelMessage = implMessage && result) |> Prop.ofTestable

            override x.ToString() = String.Concat [|"setPinCode("; pinCode; ","; newPinCode; ")" |] }


    { new ICommandGenerator<AlarmSystem,AlarmSystem> with
        member __.InitialActual = 
            let alarmSystemImpl = new AlarmSystemImpl(switchToArmedTime, switchToFlashTime, 
                                                        switchToSilentAndOpenTime, allowedWrongPinCodeCount,
                                                        allowedWrongSetPinCodeCount) :> AlarmSystem
            clearImplEvents()
            clearImplMessages()

            alarmSystemImpl.StateChanged.Add(implementationStateChangedEventHandler)
            alarmSystemImpl.MessageArrived.Add(implMessageEventHandler)
            alarmSystemImpl

        member __.InitialModel = 
            let alarmSystemModel = new AlarmSystemModel(switchToArmedTime, switchToFlashTime, 
                                                    switchToSilentAndOpenTime, allowedWrongPinCodeCount, 
                                                    allowedWrongSetPinCodeCount) :> AlarmSystem
            clearModelEvents()
            clearModelMessages()

            alarmSystemModel.StateChanged.Add(modelStateChangedEventHandler)
            alarmSystemModel.MessageArrived.Add(modelMessageEventHandler) 
            alarmSystemModel  

        member __.Next model = Gen.oneof [ gen {return getRandomShouldWait(2) |> specOpen};
                                            gen {return getRandomShouldWait(3) |> specClose};
                                            gen {return getRandomShouldWait(4) |> specLock};
                                            gen {return getRandomPin(8) |> specUnlock (getRandomShouldWait(5))};//]}
                                            gen {return getRandomPin(9) |> specSetPinCode (getRandomPin(6))} ] }


let config = {
    Config.Quick with 
        MaxTest = 5000
        //Replay = Random.StdGen (1812709121,296022046) |> Some 
    }

//Check.One(config, Command.toProperty spec)

open NUnit.Framework
open Swensen.Unquote
[<Test>]
let ``Alarm System Test``() =
    Check.QuickThrowOnFailure (Command.toProperty spec)



//Check.Verbose(asProperty spec)
//Check.One(config, (Command.toProperty spec))


