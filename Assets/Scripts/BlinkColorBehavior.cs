using Febucci.UI.Core;
using UnityEngine;

namespace Febucci.UI.Effects
{
    [UnityEngine.Scripting.Preserve]
    [CreateAssetMenu(menuName = "Text Animator/Animations/Behaviors/Blink Color", fileName = "Blink Color Behavior")]
    [EffectInfo("blink", EffectCategory.Behaviors)]
    public sealed class BlinkColorBehavior : BehaviorScriptableBase
    {
        public float baseFrequency = 1f;
        public Color blinkColor = Color.red;

        float frequency;

        public override void SetModifier(ModifierInfo modifier)
        {
            switch (modifier.name)
            {
                //frequency
                case "f": frequency = baseFrequency * modifier.value; break;
            }
        }

        public override void ResetContext(TAnimCore animator)
        {
            frequency = baseFrequency;
        }

        Color32 temp;
        public override void ApplyEffectTo(ref CharacterData character, TAnimCore animator)
        {
            float t = Mathf.PingPong(animator.time.timeSinceStart * frequency, 1f);

            for (byte i = 0; i < TextUtilities.verticesPerChar; i++)
            {
                Color32 original = character.source.colors[i];
                temp = Color32.Lerp(original, blinkColor, t);
                temp.a = original.a; //preserves original alpha
                character.current.colors[i] = temp;
            }
        }
    }
}
