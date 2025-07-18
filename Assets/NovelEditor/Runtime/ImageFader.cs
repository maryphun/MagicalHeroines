using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NovelEditor
{
    [RequireComponent(typeof(Image))]
    public class ImageFader : MonoBehaviour
    {
        private Image img;
        private float effect_time;
        private float time_cnt;
        private Gradient gradient;

        public void Init(float time, Color target)
        {
            img = GetComponent<Image>();

            effect_time = time;
            time_cnt = 0.0f;
            
            gradient = new Gradient();

            // Blend color from red at 0% to blue at 100%
            var colors = new GradientColorKey[2];
            colors[0] = new GradientColorKey(img.color, 0.0f);
            colors[1] = new GradientColorKey(target, 1.0f);

            // Blend alpha from opaque at 0% to transparent at 100%
            var alphas = new GradientAlphaKey[2];
            alphas[0] = new GradientAlphaKey(img.color.a, 0.0f);
            alphas[1] = new GradientAlphaKey(img.color.a, 1.0f);

            gradient.SetKeys(colors, alphas);

            enabled = true;
        }

        private void Update()
        {
            if (img == null)
            {
                Destroy(this);
                return;
            }

            float alpha = img.color.a;
            time_cnt = Mathf.Clamp(time_cnt + Time.deltaTime, 0.0f, effect_time);
            img.color = gradient.Evaluate(time_cnt / effect_time);
            img.color = new Color(img.color.r, img.color.g, img.color.b, alpha);

            if (time_cnt >= effect_time)
            {
                enabled = false;
                Destroy(this);
            }
        }
    }
}
