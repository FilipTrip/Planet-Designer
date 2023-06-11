using System;
using UnityEngine;
using UnityEngine.UI;
using Random = UnityEngine.Random;

namespace EasyPoolKit.Demo
{
    public class Demo2Manager : MonoBehaviour
    {
        public Text[] DemoTexts;

        public Image DemoImage;
        
        public delegate void DemoTextColorChangeHandler(object sender, ChangeColorEventArgs e);

        private event DemoTextColorChangeHandler _onTextColorChangeEvent;

        private event DemoTextColorChangeHandler _onImageColorChangeEvent;

        void Start()
        {
            foreach (var text in DemoTexts)
            {
                _onTextColorChangeEvent += (sender, args) =>
                {
                    if (args is ChangeColorEventArgs changeColorEventArgs)
                    {
                        text.color = changeColorEventArgs.TextColor;
                    }
                };
            }
            
            _onImageColorChangeEvent += OnImageColorChanged;
        }

        private void OnImageColorChanged(object sender, EventArgs e)
        {
            if (e is ChangeColorEventArgs changeColorEventArgs)
            {
                DemoImage.color = changeColorEventArgs.TextColor;
            }
        }

        private void Update()
        {
            //Send event to change text color
            var textColor = GetRandomColor();
            var textColorChangeArg = ObjectPoolKit.Spawn<ChangeColorEventArgs>();
            textColorChangeArg.TextColor = textColor;
            _onTextColorChangeEvent?.Invoke(this, textColorChangeArg);
            ObjectPoolKit.Despawn(textColorChangeArg);
            
            //Send event to change image color
            var imageColor = GetRandomColor();
            var imageColorChangeArg = ObjectPoolKit.Spawn<ChangeColorEventArgs>();
            imageColorChangeArg.TextColor = imageColor;
            _onImageColorChangeEvent?.Invoke(this, imageColorChangeArg);
            ObjectPoolKit.Despawn(imageColorChangeArg);
        }

        private Color GetRandomColor()
        {
            var randomColor = new Color(Random.Range(0f, 1f), Random.Range(0f, 1f), Random.Range(0f, 1f));
            return randomColor;
        }
    }

    public class ChangeColorEventArgs : RecyclableEventArgs
    {
        public Color TextColor = Color.white;

        public override void OnObjectDespawn()
        {
            base.OnObjectDespawn();
            TextColor = Color.white;
        }
    }
}
