namespace ES.MealyObfuscator

open System
open System.Text
open System.Collections.Generic

type Branch(startNode: Node, endNode: Node, input: Int32, output: Char) =
    member this.Start = startNode
    member this.End = endNode
    member this.Input = input
    member this.Output = output

    override this.ToString() =            
        String.Format("({0}) --{1}/'{2}'--> ({3})", this.Start.Id, this.Input, this.Output, this.End.Id)
        
and Node(label: Int32) =
    let _branches = new List<Branch>()
    let _rnd = new Random()

    let tryGetBranch(node: Node, output: Char) =
        _branches |> Seq.tryFind(fun b -> b.Output = output && b.End.Id = node.Id)

    member val NumOfUsedBranches = 0 with get, set
    member this.Id = label

    member this.IncreaseNumOfUsedBranches() =
        if this.NumOfUsedBranches < 2 then
            this.NumOfUsedBranches <- this.NumOfUsedBranches + 1

    member this.HasOutput(output: Char) =
        _branches |> Seq.exists(fun b -> b.Output = output)

    member this.GetBranches() =
        _branches |> Seq.readonly

    member this.OutputNodes(output: Char) =
        _branches 
        |> Seq.filter(fun b -> b.Output = output)
        |> Seq.map(fun b -> b.End)
                        
    member this.GetOutput(input: Int32) = 
        (_branches |> Seq.find(fun b -> b.Input = input)).Output
            
    member this.GetNextNode(input: Int32) = 
        (_branches |> Seq.find(fun b -> b.Input = input)).End
    
    member this.CreateBranch(node: Node, output: Char) =
        match tryGetBranch(node, output) with
        | Some branch -> 
            this.NumOfUsedBranches <- _branches.Count
            branch.Input
        | None -> 
            if _branches.Count = 2 then
                failwith "This node has already two branches"

            let input = 
                if _branches.Count = 0 then _rnd.Next(2)
                else ((_branches |> Seq.last).Input + 1) % 2

            let branch = new Branch(this, node, input, output)
            _branches.Add(branch)

            this.NumOfUsedBranches <- _branches.Count
            input

    override this.ToString() =
        let sb = new StringBuilder()
        sb.AppendFormat("[{0}] ", this.Id) |> ignore
        sb.Append(String.Join(", ", _branches)) |> ignore
        sb.ToString()

type Automata = {
    Input: Int32 seq
    Start: Node
}