﻿// Author: Yuuna-Project@Orlys
// Github: github.com/Orlys
// Contact: orlys@yuuna-project.com

namespace Yuuna.Contracts.Recognition.Speech
{
    using System;
    using System.Collections.Generic;

    public interface ISpeechRecognizer
    {
        event Action<IReadOnlyList<IAlternative>> RecognizeCompleted;

        IDisposable Recognize();
    }
}