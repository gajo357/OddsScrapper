namespace OddsScraper.FSharp.Common

module OptionExtension =

    let private WrapInOption v = Some v
    let private ReturnFrom t : Option<'T> = t
    let NullableToOption ot : Option<'T> =
        match ot with
        | null -> None
        | _ -> Some ot

    let private Bind ot fu =
        match ot with
        | None -> None
        | Some vt -> fu vt

    let private Bind_Nullable vt fu =
        Bind (NullableToOption vt) fu

    let private Delay ft : Option<'T> = ft ()

    type OptionBuilder() =
        member __.Return v       = WrapInOption v
        member __.ReturnFrom v   = ReturnFrom v
        member __.ReturnFrom v   = NullableToOption v
        member __.Bind (t, fu)   = Bind t fu
        member __.Bind (t, fu)   = Bind_Nullable t fu
        member __.Delay ft       = Delay ft
        member __.Zero()         = None

    let option = OptionBuilder()
    