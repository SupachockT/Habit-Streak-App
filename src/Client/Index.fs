module Index
open System 

open Elmish
open SAFE
open Shared

let habitEmojis = [
    "🏋️‍♂️"; "🏃‍♀️"; "🚴‍♂️";  // Exercise
    "🥦"; "🍎"; "🍉";          // Healthy Eating
    "🧘‍♂️"; "☕"; "🌱";        // Mindfulness
    "📚"; "✏️"; "📖";          // Reading
    "👫"; "🤝"; "🎉";          // Social Habits
    "✅"; "📝"; "⏰"           // Productivity
]

type Model = {
    IsCardHovered: bool
    IsFormOpen: bool 
    IsEmojiDropDownOpen: bool
    InputCard: InputCard option
    Habits: Habit list
}

type Msg =
    | SetCardHover of bool
    | SetFormOpen of bool
    | SetEmojiDropDown of bool
    | SaveInputCard of string * string
    | LoadHabits of Habit list 
    | CreateHabit
    | DeleteHabit of int
    | UpdateStreak of int * bool

let habitApi = Api.makeProxy<IHabitsApi> ()

let loadHabitsCmd () = Cmd.OfAsync.perform habitApi.getAllHabits () LoadHabits

let init () =
    let initialModel = { 
        IsCardHovered = false
        IsFormOpen = false
        IsEmojiDropDownOpen = false
        InputCard = None
        Habits = []
    }
    let initialCmd = loadHabitsCmd ()

    initialModel, initialCmd

let update msg model =
    match msg with
    | SetCardHover isHovered -> 
        { model with IsCardHovered = isHovered }, Cmd.none

    | SetFormOpen isOpen -> 
        let newModel = 
            if not isOpen then { model with IsFormOpen = isOpen; InputCard = None }
            else { model with IsFormOpen = isOpen }
        newModel, Cmd.none

    | SetEmojiDropDown isOpen -> 
        { model with IsEmojiDropDownOpen = isOpen }, Cmd.none

    | SaveInputCard (emoji, habit) -> 
        let newCard = { Emoji = emoji; Habit = habit }
        { model with InputCard = Some newCard }, Cmd.none

    | LoadHabits habits -> 
        { model with Habits = habits }, Cmd.none

    | CreateHabit -> 
        match model.InputCard with
        | Some inputCard ->
            let createHabitCmd = 
                Cmd.batch [
                    Cmd.OfAsync.perform habitApi.createHabit inputCard (fun _ -> LoadHabits [])
                    loadHabitsCmd() // Re-fetch the latest habit list
                ]
            { model with 
                IsFormOpen = false
                InputCard = None
            }, createHabitCmd
        | None -> 
            model, Cmd.none
            
    | DeleteHabit id -> 
        let deleteCmd = 
            Cmd.batch [
                Cmd.OfAsync.perform habitApi.deleteHabit id (fun success -> 
                    if success then LoadHabits [] else LoadHabits model.Habits)
                loadHabitsCmd() 
            ]
        model, deleteCmd

    | UpdateStreak (id, increment) ->
        let updateCmd = 
            Cmd.OfAsync.perform (fun () -> habitApi.updateStreak (id, increment)) () (fun success -> 
                if success then LoadHabits [] else LoadHabits model.Habits)
        let fetchHabitsCmd = loadHabitsCmd() // Re-fetch the latest habit list

        model, Cmd.batch [updateCmd; fetchHabitsCmd]
    
open Feliz
open Feliz.DaisyUI

module ViewComponents =
    let InputCardEmoji model dispatch =
        let emojis = habitEmojis

        let selectEmoji emoji = dispatch (SaveInputCard (emoji, ""))

        Html.div [
            prop.className "relative justify-self-start self-center ml-10"
            prop.children [
                Html.div [
                    prop.className "text-6xl flex items-center justify-center"           
                    prop.onClick ( fun _ -> dispatch (SetEmojiDropDown (not model.IsEmojiDropDownOpen)) )     
                    prop.children [
                        match model.InputCard with
                            | Some card -> Html.p card.Emoji
                            | None -> 
                                Html.button [
                                    prop.className "btn btn-neutral flex items-center"
                                    prop.children [
                                        Html.i [ prop.className "fas fa-upload mr-2" ]
                                        Html.span [ prop.text "Select" ]
                                    ]
                                ]
                    ]
                ]

                // Emoji Dropdown
                match model.IsEmojiDropDownOpen with
                    | true -> 
                        Html.div [
                            prop.className "absolute z-10 bg-white shadow-lg border border-gray-300 mt-1 rounded flex flex-wrap w-96"
                            prop.children [
                                for emoji in emojis do
                                    Html.div [
                                        prop.className "p-2 cursor-pointer hover:bg-gray-100 text-xl"
                                        prop.text emoji
                                        prop.onClick (fun _ -> selectEmoji emoji; dispatch (SetEmojiDropDown (not model.IsEmojiDropDownOpen))) // Select emoji on click
                                    ]
                            ]
                        ]
                    | _ -> ()
            ]
        ]
    let InputCardHabit model dispatch =
        let currentHabit =
            match model.InputCard with 
            | Some card -> card.Habit
            | None -> ""
        
        let isEmojiSelected = 
            match model.InputCard with
            | Some card when card.Emoji <> "" -> true
            | _ -> false

        Html.div [
            prop.className "flex items-center justify-center flex-auto"
            prop.children [
                Html.input [
                    prop.className "input input-bordered w-6/12 max-w-xs"
                    prop.placeholder "Enter your habit name"
                    prop.value currentHabit
                    prop.disabled (not isEmojiSelected)
                    prop.onChange (fun newValue ->
                        match model.InputCard with
                        | Some card -> dispatch (SaveInputCard (card.Emoji, newValue))
                        | None -> dispatch (SaveInputCard ("", newValue))
                    )
                ]

            ]
        ]

    let InputCardView model dispatch = 
        let nextPrev = 
            Html.div [
                prop.className "flex flex-col justify-between"
                prop.children [
                    Html.img [
                        prop.className "justify-self-end self-start cursor-pointer"
                        prop.src "/prev-icon.svg"
                        prop.alt "go back"
                        prop.onClick (fun ev ->
                            ev.stopPropagation()
                            dispatch (SetFormOpen false)
                        )
                    ]
                    Html.img [
                        prop.className "justify-self-end self-end cursor-pointer"
                        prop.src "/next-icon.svg"
                        prop.alt "go next"
                        prop.onClick (fun ev ->
                            ev.stopPropagation()
                            dispatch CreateHabit
                        )
                    ]
                ]
            ]

        Html.div [
            prop.className ("h-40 w-4/12 rounded shadow-xl flex bg-chineseviolet" + 
                (if model.IsCardHovered && not model.IsFormOpen then " hover:bg-mountbattenpink" else ""))
            prop.onMouseEnter (fun _ -> dispatch (SetCardHover true))
            prop.onMouseLeave (fun _ -> dispatch (SetCardHover false))
            prop.onClick (fun _ -> dispatch (SetFormOpen true))
            prop.children [
                match model.IsFormOpen with
                | true -> 
                    InputCardEmoji model dispatch
                    InputCardHabit model dispatch
                    nextPrev
                | false ->
                    Html.div [
                        prop.className "flex justify-center w-full cursor-pointer"
                        prop.children [
                            Html.img [
                                prop.src "/add-icon.svg"
                                prop.alt "Add Your Habit"
                            ]
                        ]
                    ]
            ]
        ]
    let HabitCardsView model dispatch =
        Html.div [
            prop.className "flex flex-col items-center gap-4 w-full"
            prop.children (
                model.Habits |> List.map (fun habit ->
                    let today = DateTime.Now.Date
                    let streakDate =
                        match habit.StreakDate with
                        | Some(date) -> 
                            date.Date 
                        | None -> today 
                    printfn "Today: %A" today
                    printfn "Streak Date: %A" streakDate
                    
                    let (buttonClass, buttonText) =
                        if habit.isDone && (streakDate = today) then
                            "px-2 text-white font-semibold shadow-md rounded transition-colors duration-200 bg-red-500 hover:bg-red-600", "cancel"
                        else
                            "px-2 text-white font-semibold shadow-md rounded transition-colors duration-200 bg-green-500 hover:bg-green-600", "do!"
                            
                    Html.div [
                        prop.className "relative h-40 w-4/12 rounded shadow-xl flex items-center bg-chineseviolet text-white"
                        prop.children [
                            Html.div [
                                prop.className "flex items-center gap-x-4 pl-12"
                                prop.children [
                                    Html.div [
                                        prop.className "text-5xl"
                                        prop.text habit.Emoji
                                    ]
                                    Html.div [
                                        prop.className "text-xl font-bold mt-4"
                                        prop.text habit.Name
                                    
                                    ]
                                ]
                            ]
                            Html.div [
                                prop.className "ml-40 text-xl text-yellow-400 flex flex-col items-center gap-4"
                                prop.children [
                                    Html.p [
                                        prop.text ("Streak: " + string habit.Streak + "!")
                                    ]
                                    Html.button [
                                        prop.className buttonClass
                                        prop.text buttonText
                                        prop.onClick (fun _ -> 
                                            let increment = buttonText = "do!"
                                            dispatch (UpdateStreak (habit.Id, increment))
                                            )
                                    ]
                                ]
                            ]
                            Html.div [
                                prop.className "text-white cursor-pointer absolute top-2 right-2"
                                prop.children [
                                    Html.button [
                                        prop.className "bg-red-500 text-white rounded-full w-6 h-6 flex items-center justify-center"
                                        prop.text "X"
                                        prop.onClick (fun _ -> dispatch (DeleteHabit habit.Id)) // Dispatch DeleteHabit message
                                    ]
                                ]
                            ]
                        ]
                    ]
                )
            )
        ]

let view model dispatch =
    Html.section [
        prop.className "h-screen w-screen bg-davygray"

        prop.children [
            Html.h1 [
                prop.className "py-6 mb-10 flex justify-center font-semibold text-3xl text-paledogwood"
                prop.text "HABIT STREAK APP!"
            ]

            Html.div [
                prop.className "p-4 flex flex-col items-center gap-4 border-dashed border-2 border-sage"
                prop.children [
                    ViewComponents.HabitCardsView model dispatch
                    ViewComponents.InputCardView model dispatch
                ]
            ]
        ]
    ]