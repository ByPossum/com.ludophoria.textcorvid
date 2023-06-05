using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace TextCorvid
{
    public class CharacterTextBox : TextBox
    {
        [SerializeField] private float f_frameSize, f_speed;
        [SerializeField] protected CharacterDisplayer cd_characterImage;
        [SerializeField] protected FrameDisplayer fd_frames;
        [SerializeField] protected SkippableAnimation[] A_objectsToAnimate;
        private TextGlue tg;
        private int i_currentAnimatingObject = 0;
        private IEnumerator t_currentTask;
        public CharacterDisplayer GetCharacterDisplayer { get { return cd_characterImage; } }
        public FrameDisplayer GetFrameDisplayer { get { return fd_frames; } }

        private void OnEnable()
        {
            tg = FindObjectOfType<TextGlue>();
            cas_currentState = CorvidAnimationState.idle;
        }

        private void OnDisable()
        {
            cas_currentState = CorvidAnimationState.closed;
        }

        private void Update()
        {
        }

        public void DisplayText(string _textToShow, TextDisplayType _typeToDisplay)
        {
             td_display.DisplayText(_textToShow, 0f, _typeToDisplay);
        }

        public IEnumerator ToggleTextBox(bool _opening)
        {
            t_currentTask = fd_frames.AnimateFrame(0, f_frameSize, f_speed);
            yield return StartCoroutine(t_currentTask);
            cas_currentState = CorvidAnimationState.animationEnd;
        }

        public void CloseTextBox()
        {
            t_currentTask = fd_frames.AnimateFrame(f_frameSize, 0, f_speed);
            StartCoroutine(t_currentTask);
        }
        public void ChangeCharacter(string _textID)
        {
            cd_characterImage.UpdateCharacterImage(_textID);
        }

        public override IEnumerator Interact()
        {
            switch (cas_currentState)
            {
                case CorvidAnimationState.idle:
                    cas_currentState = CorvidAnimationState.animating;
                    if (!CheckTextBoxDone())
                        Animate();
                    break;
                case CorvidAnimationState.animating:
                    Interrupt();
                    break;
                case CorvidAnimationState.animationEnd:
                    i_currentAnimatingObject++;
                    cas_currentState = CorvidAnimationState.idle;
                    StartCoroutine(Interact());
                    break;
                case CorvidAnimationState.closed:
                    ToggleTextBox(false);
                    break;
                default:
                    break;
            }
            return t_currentTask;
        }

        private void Animate()
        {
            // Find the next animatable and animate it
            switch (A_objectsToAnimate[i_currentAnimatingObject])
            {
                case FrameDisplayer frame:
                    StartCoroutine(ToggleTextBox(true));
                    break;
                case TextDisplayer dialogue:
                    DisplayText(tg.GetTextManager().GetText(s_textID), TextDisplayType.character);
                    break;
            }
        }

        private void Interrupt()
        {
            if (fd_frames.GetAnimating)
                fd_frames.SkipToTheEnd();
            else if (td_display.GetAnimating)
                td_display.SkipToTheEnd();
            cas_currentState = CorvidAnimationState.animationEnd;
        }

        private bool CheckTextBoxDone()
        {
            if (i_currentAnimatingObject >= A_objectsToAnimate.Length - 1)
            {
                ToggleTextBox(false);
                return true;
            }
            return false;
        }
    }
}