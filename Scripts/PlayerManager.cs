using R3;
using UnityEngine;
using UnityEngine.Animations;
using UnityEngine.Serialization;

namespace GameScript.Scripts
{
    public class PlayerManager : MonoBehaviour
    {
        private TriggerEventHandler _triggerEventHandler;

        [SerializeField] private GameObject sword;

        [SerializeField] private GameObject shield;

        // Start is called before the first frame update
        void Start()
        {
            _triggerEventHandler = gameObject.GetComponent<TriggerEventHandler>();
            var swordParentConstraint = sword.GetComponent<ParentConstraint>();
            var shieldParentConstraint = shield.GetComponent<ParentConstraint>();


            _triggerEventHandler.IsCombatAsObservable()
                .Subscribe(x =>
                {
                    if (x == 1)
                    {
                        Debug.Log("Player is combat");
                        ChangeParentConstraintSourceWeight(swordParentConstraint, 0, 1);
                        ChangeParentConstraintSourceWeight(swordParentConstraint, 1, 0);
                        ChangeParentConstraintSourceWeight(shieldParentConstraint, 0, 1);
                        ChangeParentConstraintSourceWeight(shieldParentConstraint, 1, 0);
                    }
                    else
                    {
                        Debug.Log("Player is not combat");
                        ChangeParentConstraintSourceWeight(swordParentConstraint, 0, 0);
                        ChangeParentConstraintSourceWeight(swordParentConstraint, 1, 1);
                        ChangeParentConstraintSourceWeight(shieldParentConstraint, 0, 0);
                        ChangeParentConstraintSourceWeight(shieldParentConstraint, 1, 1);
                    }
                });
        }

        private void ChangeParentConstraintSourceWeight(ParentConstraint parentConstraint, int arrayNumber, float weight)
        {
            var parentConstraintSource = parentConstraint.GetSource(arrayNumber);
            parentConstraintSource.weight = weight;
            parentConstraint.SetSource(arrayNumber, parentConstraintSource);
        }

        // Update is called once per frame
        void Update()
        {
        }
    }
}