using System.Collections.Generic;
using UnityEngine;

public static class Parser
{
    public static (float, int) MainParse(string textToParse, GameObject gameObject, int start = 0)
    {
        if (textToParse == null || textToParse == "") return (0, -1);

        if (textToParse.Length < 2) textToParse += " ";

        //   Debug.Log(textToParse);
        string lowerCaseLetters = "abcdefghijklmnopqrstuvwxyz";
        bool mathfParse, negative = false;
        (float value, int index) number;
        int endPoint = textToParse.Length;
        List<float> values = new();
        List<char> operators = new();

        for (int i = start; i < textToParse.Length; i++)
        {
          //  Debug.Log(textToParse[i]);
            if (textToParse[i] == ')' || textToParse[i] == ',')
            {
                endPoint = i;
               // Debug.Log("should break : " + i);
                break;
            }
            mathfParse = false;
            if (textToParse[i] == '(')
            {
              //  Debug.Log("( open");
                number = MainParse(textToParse, gameObject, i + 1);
                if (number.index == -1) return (0, -1);

                values.Add(number.value);
                i = number.index;
                continue;
            }
            foreach (char letter in lowerCaseLetters)
            {
                if (textToParse[i] == letter)
                {
                    mathfParse = true;
                    break;
                }
            }
            if (mathfParse)
            {
                number = ParseMathf(textToParse, gameObject, i);
                if (number.index == -1) return (0, -1);

                if (negative)
                {
                    values.Add(-number.value);
                    negative = false;
                }
                else
                {
                    values.Add(number.value);
                }
                i = number.index;
                continue;
            }
            switch (textToParse[i])
            {
                case 'S':
                {
                    number = ParseNumber(textToParse, i + 1);
                    if (number.index == -1) return (0, -1);

                    i = number.index;
                    if ((int)number.value - 1 < SensorManager.sensorList.Count)
                    {
                        try
                        {
                            if (negative)
                            {
                                values.Add(-SensorManager.sensorList[(int)number.value - 1].
                                    GetComponent<SensorManager>().value);
                                negative = false;
                            }
                            else
                            {
                                values.Add(SensorManager.sensorList[(int)number.value - 1].
                                    GetComponent<SensorManager>().value);
                            }
                            //Debug.Log("s value: " + SensorManager.sensorList[(int)number.value - 1].
                            //GetComponent<SensorManager>().value);
                        }
                        catch
                        {
                            return (0, -1);
                        }
                    }
                    else
                    {
                        return (0, -1);
                    }
                    break;
                }
                case '+':                  
                {
                    operators.Add('+');
                    break;
                }
                case '/':
                {
                    operators.Add('/');
                    break;
                }
                case '*':
                {
                    operators.Add('*');
                    break;
                }
                case '-':
                {
                    if (values.Count > 0 && operators.Count > 0)
                    {
                        if (textToParse.IndexOf(operators[operators.Count - 1]) <
                                textToParse.IndexOf("" + values[values.Count - 1]))
                        {
                            operators.Add('+');
                        }
                    }
                    if (operators.Count == 0 && values.Count > 0) operators.Add('+');

                    negative = !negative;
                    //Debug.Log("negative: " + negative);
                    break;
                }
                default:
                {
                    //  Debug.Log("defult index: "+i);
                    number = ParseNumber(textToParse, i);
                    if (number.index != -1)
                    {
                        i = number.index;
                        // Debug.Log("defult: " + number.value);
                        if (negative)
                        {
                            values.Add(-number.value);
                            negative = false;
                        }
                        else
                        {
                            values.Add(number.value);
                        }
                    }
                    break;
                }
            }
        }
        if (values.Count - 1 != operators.Count) return (0, -1);

        List<int> doFirst = new();
        List<int> doSecond = new();
        List<float> firstValues = new();
        List<float> secondValues = new();
        for (int i = 0; i < values.Count; i++)
        {
            if (operators.Count > i)
            {
                if (operators[i] == '/' || operators[i] == '*')
                {
                    doFirst.Add(operators[i]);
                    firstValues.Add(values[i]);
                }
                else
                {
                    doSecond.Add(operators[i]);
                    secondValues.Add(values[i]);
                }
            }
            else
            {
                if (operators.Count > 1)
                {
                    if (operators[i - 1] == '/' || operators[i - 1] == '*')
                    {
                        firstValues.Add(values[i]);
                    }
                    else
                    {
                        secondValues.Add(values[i]);
                    }
                }
                else
                {
                    firstValues.Add(values[i]);
                }
            }
        }
        firstValues.AddRange(secondValues);
        doFirst.AddRange(doSecond);
        for (int i = 0; i < doFirst.Count; i++)
        {
            switch (doFirst[i])
            {
                case '+':
                    firstValues[i + 1] = firstValues[i] + firstValues[i + 1];
                    break;
                case '/':
                    firstValues[i + 1] = firstValues[i] / firstValues[i + 1];
                    if (float.IsNaN(firstValues[i + 1])) return (0, -1);

                    break;
                case '*':
                    firstValues[i + 1] = firstValues[i] * firstValues[i + 1];
                    break;
            }
        }
        /*foreach (int symbol in doFirst)
        {
            Debug.Log("symbols : "+symbol);
        }
        foreach (float value in firstValues)
        {
            Debug.Log("values : "+value);
        }*/
        //Debug.Log("final value : " + firstValues[firstValues.Count - 1]);
        return (firstValues[^1], endPoint);
    }

    private static (float, int) ParseNumber(string textToParse, int index)
    {
        int i = index;
        string number = "";
        if (i == textToParse.Length) return (0, -1);

        while (textToParse[i] == '0' || textToParse[i] == '1' || textToParse[i] == '2' 
            || textToParse[i] == '3' || textToParse[i] == '4' || textToParse[i] == '5' 
            || textToParse[i] == '6' || textToParse[i] == '7' || textToParse[i] == '8' 
            || textToParse[i] == '9' || textToParse[i] == '.')
        {
            number += textToParse[i];
            i++;
            if (i == textToParse.Length) break;
        }
        if (i != index) return (float.Parse(number), i - 1);

        return (0, -1);
    }

    private static (float, int) ParseMathf(string textToParse, GameObject gameObject, int index)
    {
        int i = index;
        (float value, int index) number;
        string function = "";
        while (textToParse[i] != '(' && textToParse[i] != ' ' && textToParse[i] != '+' && 
            textToParse[i] != '-' && textToParse[i] != '*' && textToParse[i] != '/')
        {
            function += textToParse[i];
            i++;
            if (i == textToParse.Length) break;
        }
        int paramCount = 0;
        float[] param = new float[5];
        if (i != textToParse.Length)
        {
            if (textToParse[i] == '(')
            {
                do
                {
                    // Debug.Log("i : "+i);
                    number = MainParse(textToParse, gameObject, i + 1);
                    if (number.index == -1 || number.index == textToParse.Length || paramCount + 1 == param.Length)
                        return (0, -1);

                    paramCount++;
                    param[i] = number.value;
                    i = number.index;
                }
                while (textToParse[i] == ',');
            }
        }
        function += paramCount;
        float mathfResult;
        switch (function)
        {
            case "abs1":
                mathfResult = Mathf.Abs(param[0]);
                break;
            case "acos1":
                mathfResult = Mathf.Acos(param[0]) * Mathf.Rad2Deg;
                break;
            case "asin1":
                mathfResult = Mathf.Asin(param[0]) * Mathf.Rad2Deg;
                break;
            case "atan1":
                mathfResult = Mathf.Atan(param[0]) * Mathf.Rad2Deg;
                break;
            case "atan22":
                mathfResult = Mathf.Atan2(param[0], param[1]) * Mathf.Rad2Deg;
                break;
            case "ceil1":
                mathfResult = Mathf.Ceil(param[0]);
                break;
            case "clamp3":
                mathfResult = Mathf.Clamp(param[0], param[1], param[2]);
                break;
            case "clamp011":
                mathfResult = Mathf.Clamp01(param[0]);
                break;
            case "cos1":
                mathfResult = Mathf.Cos(param[0] * Mathf.Deg2Rad);
                break;
            case "deltaAngle2":
                mathfResult = Mathf.DeltaAngle(param[0], param[1]);
                break;
            case "exp1":
                mathfResult = Mathf.Exp(param[0]);
                break;
            case "floor1":
                mathfResult = Mathf.Floor(param[0]);
                break;
            case "inverseLerp3":
                mathfResult = Mathf.InverseLerp(param[0], param[1], param[2]);
                break;
            case "lerp3":
                mathfResult = Mathf.Lerp(param[0], param[1], param[2]);
                break;
            case "lerpAngle3":
                mathfResult = Mathf.LerpAngle(param[0], param[1], param[2]);
                break;
            case "lerpUnclamped3":
                mathfResult = Mathf.LerpUnclamped(param[0], param[1], param[2]);
                break;
            case "log1":
                mathfResult = Mathf.Log(param[0]);
                break;
            case "log101":
                mathfResult = Mathf.Log10(param[0]);
                break;
            case "max1":
                mathfResult = Mathf.Max(param[0]);
                break;
            case "min1":
                mathfResult = Mathf.Min(param[0]);
                break;
            case "moveTowards3":
                mathfResult = Mathf.MoveTowards(param[0], param[1], param[2]);
                break;
            case "moveTowardsAngle3":
                mathfResult = Mathf.MoveTowardsAngle(param[0], param[1], param[2]);
                break;
            case "pingPong2":
                mathfResult = Mathf.PingPong(param[0], param[1]);
                break;
            case "pow2":
                mathfResult = Mathf.Pow(param[0], param[1]);
                break;
            case "repeat2":
                mathfResult = Mathf.Repeat(param[0], param[1]);
                break;
            case "round1":
                mathfResult = Mathf.Round(param[0]);
                break;
            case "sign1":
                mathfResult = Mathf.Sign(param[0]);
                break;
            case "sin1":
                mathfResult = Mathf.Sin(param[0] * Mathf.Deg2Rad);
                break;
            case "smoothDamp4":
                mathfResult = Mathf.SmoothDamp(param[0], param[1], ref param[2], param[3]);
                break;
            case "smoothDamp5":
                mathfResult = Mathf.SmoothDamp(param[0], param[1], ref param[2], param[3], param[4]);
                break;
            case "smoothDampAngle4":
                mathfResult = Mathf.SmoothDampAngle(param[0], param[1], ref param[2], param[3]);
                break;
            case "smoothDampAngle5":
                mathfResult = Mathf.SmoothDampAngle(param[0], param[1], ref param[2], param[3], param[4]);
                break;
            case "smoothStep3":
                mathfResult = Mathf.SmoothStep(param[0], param[1], param[2]);
                break;
            case "sqrt1":
                mathfResult = Mathf.Sqrt(param[0]);
                break;
            case "tan1":
                mathfResult = Mathf.Tan(param[0] * Mathf.Deg2Rad);
                break;
            case "time0":
                mathfResult = RoundManager.timeSinceBattleStart + (RoundManager.buildTime + RoundManager.battleTime)
                    * gameObject.GetComponent<SetupManager>().numBattles;
                break;
            case "deg2Rad0":
                mathfResult = Mathf.Deg2Rad;
                break;
            case "pi0":
                mathfResult = Mathf.PI;
                break;
            case "rad2Deg0":
                mathfResult = Mathf.Rad2Deg;
                break;
            default:
                return (0, -1);
        }
        if (float.IsNaN(mathfResult)) return (0, -1);

        Debug.Log(mathfResult);

        return (mathfResult, i);
    }
}
