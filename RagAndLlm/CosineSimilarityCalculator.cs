using System;
using System.Collections.Generic;
using System.Linq;

public class CosineSimilarity
{
    private List<int> CreateVector(List<string> tokens, List<string> vocabulary)
    {
        //this counts how many times each word from the user input occurs in the vocabulary
        //In this case, the vocabulary is the standard situations
        return vocabulary.Select(word => tokens.Count(t => t == word)).ToList();
    }

    public double CalculateCosineSimilarity(string userInput, string standardSituation, TextProcessor textProcessor)
    {
        //Splits the inputs into separate words/tokens
        var userInputTokens = textProcessor.Tokenize(userInput);
        var standardSituationTokens = textProcessor.Tokenize(standardSituation);

        // Create a combined vocabulary
        var vocabulary = userInputTokens.Union(standardSituationTokens).Distinct().ToList();

        // Create vectors for the actual calculation
        var userVector = CreateVector(userInputTokens, vocabulary);
        var standardSituationVector = CreateVector(standardSituationTokens, vocabulary);

        // Calculate dot product and magnitudes
        double dotProduct = userVector.Zip(standardSituationVector, (x, y) => x * y).Sum();
        double userMagnitude = Math.Sqrt(userVector.Sum(x => x * x));
        double standardSituationMagnitude = Math.Sqrt(standardSituationVector.Sum(x => x * x));

        // Return cosine similarity
        return dotProduct / (userMagnitude * standardSituationMagnitude);
    }
}