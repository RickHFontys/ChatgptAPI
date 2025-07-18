using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using TMPro;
using UnityEngine;

public class RAGLogic : MonoBehaviour
{
    static public RAGLogic Instance;

    private List<StandardSituations> _situations = new List<StandardSituations>();
    private List<CardsData> _currentCards;

    void Awake()
    {
        RAGLogic.Instance = this;
    }

    void Start()
    {
        ParseJson();
    }

    public string GiveAdvice(string prompt)
    {
        _currentCards = SaveChosenCards.LoadChosenCards(true);
        Debug.Log(prompt);
        if (prompt == string.Empty)
        {
            return "";
        }
        foreach (var card in _currentCards)
        {
            prompt += " " + card.ChosenOption;
        }
        var tokenizer = new TextProcessor();
        var similarityCalculator = new CosineSimilarity();

        // Calculate similarities for each standard situation
        Dictionary<StandardSituations, double> similarities = new Dictionary<StandardSituations, double>();
        foreach (var situation in _situations)
        {
            double similarity = similarityCalculator.CalculateCosineSimilarity(prompt, situation._situation, tokenizer);
            similarities[situation] = similarity;
        }

        foreach (var kvp in similarities.OrderByDescending(kvp => kvp.Value))
        {
            string advice = kvp.Key._advice;
            if (kvp.Value == similarities.OrderByDescending(kvp => kvp.Value).ElementAt(1).Value)
            {
                string otherOption = similarities.OrderByDescending(kvp => kvp.Value).ElementAt(1).Key._advice.ToLower(); 
                advice += " If that doesn't work, " + otherOption;
            }
            
            return advice;
        }

        return "";
    }

    private void ParseJson()
    {
        string json = Resources.Load<TextAsset>("RAGContext/Situations").text;
        var data = JsonConvert.DeserializeObject<Dictionary<string, string>>(json);

        foreach (var situation in data)
        {
            string situationType = situation.Key;
            string advice = situation.Value;

            _situations.Add(new StandardSituations(situationType, advice));
        }
    }
}
