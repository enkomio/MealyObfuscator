namespace MealyObfuscatorTest

open System
open System.Text
open System.Linq
open ES.MealyObfuscator

module Tests =
    let private _rnd = new Random()

    let run(fsm: Node, inputList: Int32 seq) =
        let mutable node = fsm
        let sb = new StringBuilder()
        for input in inputList do
            let output = node.GetOutput(input)
            node <- node.GetNextNode(input)
            sb.Append(output) |> ignore
        sb.ToString()

    let test(text: String) =
        let automataGenerator = new MealyAutomata()

        // create machine
        let automata = automataGenerator.CreateMealyAutomata(text)

        // test computed input
        let deobfuscatedString = run(automata.Start, automata.Input)

        if not(text.Equals(deobfuscatedString, StringComparison.Ordinal)) then
            failwith ("Test failed with string: " + text)

    let generateRandomString(length: Int32) =
        let chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789"
        new String(Enumerable.Repeat(chars, length).Select(fun s -> s.[_rnd.Next(s.Length)]).ToArray())
            
    let runTests() =
        Console.WriteLine("-= START TEST =-")        
        for i=0 to 1000 do
            let randomString = generateRandomString(_rnd.Next(10, 250))
            Console.Write("Test {0}: {1} => ", i, randomString)
            test(randomString)            
            Console.WriteLine("OK")
        Console.WriteLine("-= TEST COMPLETED =-")
        
    [<EntryPoint>]
    let main argv = 
        runTests()
        0
