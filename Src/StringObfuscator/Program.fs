open System
open System.Text
open System.Linq
open System.Collections.Generic
open Argu
open ES.MealyObfuscator
        
module Program =
    type CLIArguments =
        | [<MainCommand; ExactlyOnce; Last>] Text of text:String
    with
        interface IArgParserTemplate with
            member s.Usage =
                match s with                
                | Text _ -> "the string that will be obfuscated."

    let printBanner() =
        Console.ForegroundColor <- ConsoleColor.Cyan        
        let banner = "-=[ Mealy String Obfuscator ]=-"
        let year = if DateTime.Now.Year = 2017 then "2017" else String.Format("2017-{0}", DateTime.Now.Year)
        let copy = String.Format("Copyright (c) {0} Antonio Parata - @s4tan{1}", year, Environment.NewLine)
        
        Console.WriteLine()
        Console.WriteLine(banner.PadLeft(abs(banner.Length - copy.Length) / 2 + banner.Length))
        Console.WriteLine(copy)
        Console.ResetColor()

    let printUsage(body: String) =
        Console.WriteLine(body)

    let printError(errorMsg: String) =
        Console.ForegroundColor <- ConsoleColor.Red
        Console.WriteLine(errorMsg)
        Console.ResetColor()    

    let rec dumpBranches(node: Node, branches: List<String>, out: List<String>, visitedNodes: HashSet<Int32>) =
        let zero = node.GetNextNode(0)
        let one = node.GetNextNode(1)
        branches.Add(String.Format("{{{0}, {1}}}", zero.Id, one.Id))
        out.Add(String.Format("{{'{0}', '{1}'}}", node.GetOutput(0), node.GetOutput(1)))

        for branch in node.GetBranches() do
            if visitedNodes.Add(branch.Start.Id) then
                dumpBranches(branch.Start, branches, out, visitedNodes)
            if visitedNodes.Add(branch.End.Id) then
                dumpBranches(branch.End, branches, out, visitedNodes)
                
    let rec getAllNodes(ids: HashSet<Int32>, node: Node) = seq {
        if ids.Add(node.Id) then
            yield node
            yield! getAllNodes(ids, node.GetNextNode(0))
            yield! getAllNodes(ids, node.GetNextNode(1))
    }
        
    let obfuscateString(text: String, args: ParseResults<CLIArguments>) =
        let mealyGenerator = new MealyAutomata()
        let automata = mealyGenerator.CreateMealyAutomata(text)

        let next = new List<String>()
        let out = new List<String>()
        let allNodeIds = new HashSet<Int32>()

        getAllNodes(allNodeIds, automata.Start)
        |> Seq.sortBy(fun n -> n.Id)
        |> Seq.iteri(fun i node ->
            let zero = node.GetNextNode(0)
            let one = node.GetNextNode(1)
            next.Add(String.Format("{{{0}, {1}}}", zero.Id, one.Id))
            out.Add(String.Format("{{'{0}', '{1}'}}", node.GetOutput(0), node.GetOutput(1)))
        )

        Console.WriteLine("Input text: " + text)
        if automata.Input.Count() < 32 then            
            let inputString = String.Join(String.Empty, automata.Input |> Seq.rev)
            Console.WriteLine("Input: {0}, Int: {1}", String.Join(",", automata.Input), Convert.ToInt32(inputString, 2))
        else
            Console.WriteLine("Input: {0}", String.Join(",", automata.Input |> Seq.rev))
                    
        let outputchars = String.Join(", ", out)        
        Console.WriteLine("Output: {{{0}}}", outputchars)     
        
        let automata = String.Join(", ", next)
        Console.WriteLine("Automata: {{{0}}}", automata)

    [<EntryPoint>]
    let main argv =
        printBanner()

        let parser = ArgumentParser.Create<CLIArguments>()
        try            
            let results = parser.Parse(argv)
                    
            if results.IsUsageRequested then
                printUsage(parser.PrintUsage())
                0
            else                
                match results.TryGetResult(<@ Text @>) with                
                | Some text ->
                    obfuscateString(text, results)
                    0
                | None ->
                    printUsage(parser.PrintUsage())   
                    1
        with 
            | :? ArguParseException ->
                printUsage(parser.PrintUsage())   
                1
            | e ->
                printError(e.Message)
                1
        
