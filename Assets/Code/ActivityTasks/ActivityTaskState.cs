using UnityEngine;

namespace Code.ActivityTasks
{
    public class ActivityTaskState
    {
        public ActivityTaskDefinition Definition { get; }
        public float Duration { get; }
        public float Remaining { get; private set; }
        public bool HasStarted { get; private set; }
        public bool IsOverdue { get; private set; }

        public ActivityTaskState(ActivityTaskDefinition definition)
        {
            Definition = definition;
            Duration = definition.StartDeadline;
            Remaining = Duration;
        }

        public void MarkStarted()
        {
            HasStarted = true;
        }

        // Returns true only on the transition into an overdue state.
        public bool Advance(float deltaTime)
        {
            if (Definition.ForceStart || HasStarted || IsOverdue)
                return false;

            Remaining = Mathf.Max(0f, Remaining - Mathf.Max(0f, deltaTime));
            if (Remaining > 0f)
                return false;

            IsOverdue = true;
            return true;
        }
    }
}
