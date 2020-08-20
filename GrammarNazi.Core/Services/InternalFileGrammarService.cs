﻿using GrammarNazi.Core.Extensions;
using GrammarNazi.Domain.Entities;
using GrammarNazi.Domain.Enums;
using GrammarNazi.Domain.Services;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace GrammarNazi.Core.Services
{
    public class InternalFileGrammarService : IGrammarService
    {
        private readonly IFileService _fileService;
        private readonly IStringDiffService _stringDiffService;
        private readonly ILanguageService _languageService;

        public GrammarAlgorithms GrammarAlgorith => GrammarAlgorithms.InternalAlgorithm;

        public InternalFileGrammarService(IFileService fileService,
            IStringDiffService stringDiffService,
            ILanguageService languageService)
        {
            _fileService = fileService;
            _stringDiffService = stringDiffService;
            _languageService = languageService;
        }

        public Task<GrammarCheckResult> GetCorrections(string text)
        {
            var languageInfo = _languageService.IdentifyLanguage(text);

            // TODO: Implement spanish corrections
            if (languageInfo.ThreeLetterISOLanguageName == SupportedLanguages.Spanish.GetDescription())
                return Task.FromResult(new GrammarCheckResult(null));

            var dictionary = _fileService.GetTextFileByLine("Library/words_en-US.txt");
            var names = _fileService.GetTextFileByLine("Library/names.txt");
            var dictionaryAndNames = dictionary.Union(names.Select(v => v.ToLower()));

            var corrections = new List<GrammarCorrection>();

            var words = text.Split(" ");

            foreach (var item in words)
            {
                // Remove special characters
                var word = Regex.Replace(item, "[^0-9a-zA-Z]+", "").ToLower();

                if (string.IsNullOrEmpty(word))
                {
                    continue;
                }

                var wordFound = dictionaryAndNames.Any(v => v == word);

                if (!wordFound)
                {
                    var possibleCorrections = dictionaryAndNames.Where(v => _stringDiffService.IsInComparableRange(v, word) && _stringDiffService.ComputeDistance(v, word) < 2);

                    if (possibleCorrections.Any())
                    {
                        corrections.Add(new GrammarCorrection { WrongWord = item, PossibleReplacements = possibleCorrections });
                    }
                }
            }

            return Task.FromResult(new GrammarCheckResult(corrections));
        }
    }
}