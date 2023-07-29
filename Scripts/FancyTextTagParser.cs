using System.Collections.Generic;
using System.Text.RegularExpressions;

public static class FancyTextTagParser
{
    static readonly string tagPattern = @"(?<=\<)(.*?)(?=\>)";
    static readonly string wholeTagPattern = @"<(.*?)>";
    static readonly string spacesNotInTagsPattern = @" +(?![^<]*\>)";
    static readonly string tagNamePattern = @"([^/|<]*?)(?=\>|,|=)";

    static readonly RegexOptions rxOptions = RegexOptions.Multiline | RegexOptions.IgnoreCase;

    static readonly Regex tagRX = new Regex(tagPattern, rxOptions);
    static readonly Regex wholeTagRX = new Regex(wholeTagPattern, rxOptions);
    static readonly Regex spacesNotInTagsRX = new Regex(spacesNotInTagsPattern, rxOptions);
    static readonly Regex tagNameRX = new Regex(tagNamePattern, rxOptions);

    static FancyTextSettingsAsset settingsAsset;

    public static List<ParsedTag> ParseTags(string text)
    {
        List<ParsedTag> parsedTags = new List<ParsedTag>();

        MatchCollection allTags = tagRX.Matches(text);
        Match[] matchArray = new Match[allTags.Count]; allTags.CopyTo(matchArray, 0);
        List<Match> unparsedTags = new List<Match>(matchArray);

        while(unparsedTags.Count > 0)
        {
            int tagCloseIndexInUnparsedList = GetFirstClosingTagIndex(unparsedTags);
            if (tagCloseIndexInUnparsedList == -1) { UnityEngine.Debug.LogError("Tag(s) missing closing tag!"); return parsedTags; }
            string tagName = unparsedTags[tagCloseIndexInUnparsedList].Value.Substring(1);

            int tagOpenIndexInUnparsedList = GetClosestMatchingOpenTag(tagName, tagCloseIndexInUnparsedList, unparsedTags);
            if (tagOpenIndexInUnparsedList == -1) { UnityEngine.Debug.LogError("Tag(s) missing opening tag!"); return parsedTags; }

            // Creating new parsed tag
            int openTagIndexOffset = TotalMatchLengthAndSpacesUntil(text, unparsedTags[tagOpenIndexInUnparsedList].Index) + 1; // honestly no clue why i have to offset 1 and 2 but it works so idc
            int closeTagIndexOffset = TotalMatchLengthAndSpacesUntil(text, unparsedTags[tagCloseIndexInUnparsedList].Index) + 2;

            TextEffectParameter[] parameters = ParseParameters(unparsedTags[tagOpenIndexInUnparsedList].Value.Split(", "));
            ParsedTag parsed = new ParsedTag(tagName, parameters, unparsedTags[tagOpenIndexInUnparsedList].Index - openTagIndexOffset, unparsedTags[tagCloseIndexInUnparsedList].Index - closeTagIndexOffset);
            parsedTags.Add(parsed);

            // Removing parsed tags from unparsed tag list
            unparsedTags.RemoveAt(tagCloseIndexInUnparsedList);
            unparsedTags.RemoveAt(tagOpenIndexInUnparsedList);
        }

        return parsedTags;
    }

    static int GetClosestMatchingOpenTag(string closeTagName, int closeTagIndex, List<Match> tags)
    {
        for (int i = closeTagIndex - 1; i >= 0; i--)
        {
            if (TagName(tags[i].Value).Equals(closeTagName) && !IsClosingTag(tags[i].Value))
            {
                return i;
            }
        }

        return -1;
    }

    static int GetFirstClosingTagIndex(List<Match> tags)
    {
        for (int i = 0; i < tags.Count; i++)
        {
            if (IsClosingTag(tags[i].Value)) { return i; }
        }

        return -1;
    }

    static bool IsClosingTag(string tag){ return tag[0] == '/';}

    static int TotalMatchLengthAndSpacesUntil(string text, int until)
    {
        string untilText = text.Substring(0, until);
        int spaces = spacesNotInTagsRX.Matches(untilText).Count;

        int matchTotal = 0;
        MatchCollection matches = wholeTagRX.Matches(untilText);
        for (int i = 0; i < matches.Count; i++) { matchTotal += matches[i].Length; }

        UnityEngine.Debug.Log("Spaces: " + spaces + " | Match Length: " + matchTotal);

        return spaces + matchTotal;
    }

    static void LogUnparsedTags(List<Match> unparsedTags)
    {
        for (int i = 0; i < unparsedTags.Count; i++)
        {
            UnityEngine.Debug.Log(unparsedTags[i]);
        }
    }

    static string TagName(string tag) { return tag.Split(", ")[0]; }

    static TextEffectParameter[] ParseParameters(string[] paramStrings)
    {
        // index 0 is always the name (if formatted correctly)
        if (paramStrings.Length > 1)
        {
            TextEffectParameter[] parameters = new TextEffectParameter[paramStrings.Length - 1];

            for (int i = 1; i < paramStrings.Length; i++)
            {
                string[] splitParam = paramStrings[i].Split(": ");
                parameters[i - 1] = new TextEffectParameter(splitParam[0], float.Parse(splitParam[1]));
            }

            return parameters;
        }

        return null;
    }

    public static string RemoveTags(string input, FancyTextSettingsAsset settingsAsset) 
    {
        FancyTextTagParser.settingsAsset = settingsAsset;
        return wholeTagRX.Replace(input, new MatchEvaluator(FancyTextTagParser.ReplaceOnlyFancyTextTags)); 
    }
    static string ReplaceOnlyFancyTextTags(Match m)
    {
        string tagName = tagNameRX.Match(m.Value).Value;
        return settingsAsset.IsRecognizedTag(tagName) ? "" : m.Value;
    }
}

public class ParsedTag
{
    public readonly string EffectName;
    public readonly TextEffectParameter[] Parameters;
    public readonly int ParsedTagStartIndex;
    public int ParsedTagEndIndex;

    public ParsedTag(string EffectName, TextEffectParameter[] Parameters, int ptStartIndex, int ptEndIndex)
    {
        this.EffectName = EffectName;
        this.Parameters = Parameters;
        ParsedTagStartIndex = ptStartIndex;
        ParsedTagEndIndex = ptEndIndex;
    }

    public override string ToString()
    {
        return "(\"" + EffectName + "\", Parameters: " + (Parameters?.Length ?? 0) + ", " + ParsedTagStartIndex + " to " + ParsedTagEndIndex + ")";
    }
}