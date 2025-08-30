using UnityEngine.UIElements;

namespace DistractorTask.Editor.UI.DuplicatePropertyDrawer
{
    public class DuplicateField : VisualElement
    {
        public void SetDuplicate(UserStudyEvaluationTextBased.Duplicate duplicate)
        {
            _duplicate = duplicate;
            _userId.text = $"{_duplicate.fileName}: {_duplicate.count.ToString()}";
        }

        private UserStudyEvaluationTextBased.Duplicate _duplicate = new();
        private Label _userId;

        public DuplicateField()
        {
            _userId = new Label(_duplicate.fileName);
                
            Add(_userId);
        }
    }
}