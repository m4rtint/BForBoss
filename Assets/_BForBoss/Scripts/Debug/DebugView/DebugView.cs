using UnityEngine;

namespace BForBoss
{
    public abstract class DebugView
    {
        protected Rect _masterRect;
        protected Rect _baseRect;
        protected Rect _backButtonRect;

        private const float _backButtonHeight = 20f;

        public Rect MasterRect
        {
            set
            {
                _masterRect = value;
                CreateBaseRect();
            }
        }

        public virtual void ResetData()
        {
            DebugWindow.OnGUIUpdate -= OnGUIUpdate;
        }

        protected abstract void DrawWindow(int id);

        protected virtual void CreateBaseRect()
        {
            _baseRect = new Rect(_masterRect.x, _backButtonHeight, _masterRect.width,
                _masterRect.height - _backButtonHeight);
            _backButtonRect = new Rect(_masterRect.x, 0, _masterRect.width, _backButtonHeight);
        }

        protected virtual void DrawGUI()
        {
            
        }


        private void OnGUIUpdate()
        {
            DrawBackButton();
            DrawGUI();
        }

        private void DrawBackButton()
        {
            using (new GUILayout.AreaScope(_backButtonRect))
            {
                if (GUILayout.Button("Back", GUILayout.Width(_backButtonRect.width)))
                {
                    ResetData();
                }
            }
        }
    }
}
