using System;
using System.Collections.Generic;
using System.Linq;

public class TextProcessor 
{
    public List<string> Tokenize(string input)
    {
        //this splits the user question into words and removes some special characters
        return input.ToLower()
                    .Split(new[] { ' ', '.', ',', '!', '?' }, StringSplitOptions.RemoveEmptyEntries)
                    .ToList();
    }


}