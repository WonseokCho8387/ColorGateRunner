using System;

namespace ColorGateRunner.Product
{
    // Attempt and route state deliberately expires with the running Player.
    public sealed class PlaytestSession
    {
        private static readonly string[] Route =
        {
            "stage-01", "stage-02", "stage-03", "stage-06",
            "stage-09", "stage-21", "stage-18", "stage-24"
        };

        public int StageCount => Route.Length;
        public int Position { get; private set; }
        public string StageId => Route[Position];
        public int RetryCount { get; private set; }
        public bool StageCompleted { get; private set; }
        public bool Completed => Position == Route.Length - 1 && StageCompleted;
        public bool AttemptStarted { get; private set; }
        public bool HasFirstInput { get; private set; }
        public bool TutorialCompleted { get; private set; }
        private bool _tutorialInput;
        private bool _tutorialMatch;

        public bool BeginAttempt()
        {
            if (AttemptStarted || StageCompleted) return false;
            AttemptStarted = true;
            return true;
        }

        public void RecordInput()
        {
            if (!AttemptStarted || StageCompleted) return;
            HasFirstInput = true;
            if (Position == 0) _tutorialInput = true;
        }

        public void RecordDirectMatch()
        {
            if (AttemptStarted && !StageCompleted && Position == 0 && _tutorialInput)
                _tutorialMatch = true;
        }

        public bool CompleteStage(string stageId)
        {
            if (!AttemptStarted || StageCompleted || stageId != StageId) return false;
            StageCompleted = true;
            if (Position == 0) TutorialCompleted = _tutorialInput && _tutorialMatch;
            return true;
        }

        public bool Retry()
        {
            if (!AttemptStarted || StageCompleted) return false;
            RetryCount++;
            AttemptStarted = false;
            _tutorialInput = false;
            _tutorialMatch = false;
            return true;
        }

        public bool Advance()
        {
            if (!StageCompleted || Completed) return false;
            Position++;
            RetryCount = 0;
            AttemptStarted = false;
            StageCompleted = false;
            return true;
        }

        public void Restart()
        {
            Position = 0;
            RetryCount = 0;
            StageCompleted = false;
            AttemptStarted = false;
            HasFirstInput = false;
            TutorialCompleted = false;
            _tutorialInput = false;
            _tutorialMatch = false;
        }
    }

    public sealed class PlaytestMemorySaveService : ILocalSaveService
    {
        private LocalSaveData _saved;

        public LocalSaveLoadResult Load() => LocalSaveLoadResult.Success(
            _saved?.Clone() ?? LocalSaveData.CreateEmpty(), _saved == null, false);

        public LocalSaveWriteResult Save(LocalSaveData data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            _saved = data.Clone();
            return LocalSaveWriteResult.Success(SaveReplacementResult.None);
        }
    }
}
