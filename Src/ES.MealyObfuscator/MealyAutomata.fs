namespace ES.MealyObfuscator

open System
open System.Text
open System.Linq
open System.Collections.Generic

type MealyAutomata() =
    let _rnd = new Random()

    let getNodes(fsm: List<Node>, output: Char) = seq {
        for node in fsm do
            if node.HasOutput(output) then
                yield node
    }

    let createNode(fsm: List<Node>) =
        let node = new Node(fsm.Count)
        fsm.Add(node)
        node

    let rec haveGoodCandidates(fsm: List<Node>, node: Node, text: String) =
        if String.IsNullOrEmpty(text) then true
        elif node.NumOfUsedBranches < 2 then true
        else
            let output = text.[0]
            if node.HasOutput(output) then
                if text.Length = 1 then true
                else
                    node.OutputNodes(output)
                    |> Seq.exists(fun nextNode -> haveGoodCandidates(fsm, nextNode, text.Substring(1)))
            else false

    let tryGetConnectedNode(fsm: List<Node>, node: Node, text: String) =
        let output = text.[0]
        let remainText = text.Substring(1)
        
        node.OutputNodes(output)
        |> Seq.tryFind(fun nextNode -> haveGoodCandidates(fsm, nextNode, remainText))

    let isSelfLoop(node: Node, currentOuput: Char, remainText: String) = 
        if remainText.Length > 0 then
            let nextOutput = remainText.[0]
            currentOuput = nextOutput && node.NumOfUsedBranches = 1
        else
            false

    let tryGetFreeNode(currentNode: Node, output: Char, remainText: String, fsm: List<Node>) =        
        if isSelfLoop(currentNode, output, remainText) then 
            // the string contains repeated characters. Better to create a loop
            Some currentNode
        elif remainText.Length = 0 then
            // I have to connect last node, try to find one that has that output
            fsm |> Seq.tryFind(fun n -> n.HasOutput(output))
        else 
            // worst case, try to find a node with no output branch
            fsm |> Seq.tryFind(fun n -> n.NumOfUsedBranches = 0)

    let tryFindOptimalNode(fsm: List<Node>, output: Char, text: String) =
        getNodes(fsm, output)
        |> Seq.tryFind(fun outputNode -> haveGoodCandidates(fsm, outputNode, text))

    let rec buildFsm(fsm: List<Node>, inputStream: List<Int32>, node: Node, text: String) =
        if not <| String.IsNullOrEmpty(text) then            
            let output = text.[0]
            let remainText = text.Substring(1)

            match tryGetConnectedNode(fsm, node, text) with
            | Some nextNode ->
                // follow an existing branch of the current node
                inputStream.Add(node.CreateBranch(nextNode, output))
                buildFsm(fsm, inputStream, nextNode, remainText)

            | None when text.Length > 1 ->
                // try to find an optimal node that I can connect to
                node.IncreaseNumOfUsedBranches()
                let nextOutput = remainText.[0]
                
                match tryFindOptimalNode(fsm, nextOutput, remainText) with
                | Some optimalNode -> 
                    // create a branch to an existing node
                    inputStream.Add(node.CreateBranch(optimalNode, output))
                    buildFsm(fsm, inputStream, optimalNode, remainText)

                | None -> 
                    connectToSubOptimalNode(fsm, inputStream, node, text)

            | None ->
                connectToSubOptimalNode(fsm, inputStream, node, text)

    and connectToSubOptimalNode(fsm: List<Node>, inputStream: List<Int32>, node: Node, text: String) =
        let output = text.[0]
        let remainText = text.Substring(1)

        match tryGetFreeNode(node, output, remainText, fsm) with
        | Some nextNode ->
            // use a random node which has no out branches
            inputStream.Add(node.CreateBranch(nextNode, output))
            buildFsm(fsm, inputStream, nextNode, remainText)
        | None ->
            // create a new node
            let newNode = createNode(fsm)
            inputStream.Add(node.CreateBranch(newNode, output))
            buildFsm(fsm, inputStream, newNode, remainText)

    let addBogusMissingBranches(fsm: List<Node>, text: String) =
        for node in fsm do
            while node.NumOfUsedBranches < 2 do
                let randomNode = fsm.[_rnd.Next(fsm.Count)]                
                let mutable randomOutput = text.[_rnd.Next(text.Length)]
                while node.HasOutput(randomOutput) do
                    randomOutput <- text.[_rnd.Next(text.Length)]
                node.CreateBranch(randomNode, randomOutput) |> ignore

    member this.CreateMealyAutomata(text: String) =
        let inputStream = new List<Int32>()
        let fsm = new List<Node>()
        let start = createNode(fsm)
        buildFsm(fsm, inputStream, start, text)
        addBogusMissingBranches(fsm, text)
        {
            Start = start
            Input = inputStream
        }
