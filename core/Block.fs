﻿module internal rec Block

open Nonempty
open Extensions


///////////////////////////////////////////////////////////////////////////////
// TYPES
///////////////////////////////////////////////////////////////////////////////

type Blocks = Nonempty<Block>

type Block =
    | Wrap of TextType * Wrappable
    | Ignore of int

type TextType =
    | Comment of (Lines -> Blocks)
    | Text
    | Code

type Wrappable =
    Prefixes * Lines

module Wrappable =
    let mapPrefixes = Tuple.mapFirst
    let mapLines = Tuple.mapSecond

/// A tuple of two strings. The first represents the prefix used for the first
/// line of a block of lines; the second the prefix for the rest. Some blocks,
/// eg a list item or a block comment, will have a different prefix for the
/// first line than for the rest. Others have the same for both.
type Prefixes =
    string * string

type Lines = 
    Nonempty<string>


///////////////////////////////////////////////////////////////////////////////
// CONSTRUCTORS
///////////////////////////////////////////////////////////////////////////////

let comment parser wrappable: Block =
    Wrap(Comment parser, wrappable)

let text wrappable: Block =
    Wrap(Text, wrappable)

let code wrappable: Block =
    Wrap(Code, wrappable)

let ignore lines: Block =
    Ignore(Nonempty.length lines)


///////////////////////////////////////////////////////////////////////////////
// GETTING INFO FROM BLOCKS
///////////////////////////////////////////////////////////////////////////////

/// Gets the length of a block
let length block =
    match block with
        | Wrap (_, (_, lines)) ->
            Nonempty.length lines

        | Ignore n ->
            n

// Returns whether the block is an ignore block
let isIgnore block =
    match block with
        | Ignore _ ->
            true

        | _ ->
            false


///////////////////////////////////////////////////////////////////////////////
// MODIFYING BLOCKS
///////////////////////////////////////////////////////////////////////////////

let splitUp (parser: Lines -> Blocks) ((pHead, pTail), lines) =

    let concatPrefixes (head1, tail1) (head2, tail2) =
        (head1 + head2, tail1 + tail2)

    let prependPrefixes p block =
        match block with
            | Wrap (t, w) ->
                Wrap (t, w |> Tuple.mapFirst (concatPrefixes p))

            | Ignore _ ->
                block
    
    parser lines
        |> Nonempty.mapHead (prependPrefixes (pHead, pTail))
        |> Nonempty.mapTail (prependPrefixes (pTail, pTail))
