using ArtifactOfRevolution;
using IL.RoR2.ExpansionManagement;
using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine.UIElements;

namespace ArtifactOfRevolution
{
    public class WeightedTypeCollection<T>
    {
        public T SelectByWeight(System.Random generatorRandom)
        {
            List<int> pickable = new List<int>();


            int Weight = 0;
            for (int i = 0; i < this.elements.Length; i++)
            {
                WeightedType<T> weightedInt = this.elements[i];

                RoR2.ExpansionManagement.ExpansionDef requiredExpansion = weightedInt.requiredExpansion;
                if (requiredExpansion != null)
                {
                    if (RoR2.Run.instance.IsExpansionEnabled(requiredExpansion))
                    {
                        if (weightedInt.Weight > 0f)
                        {
                            pickable.Add(i);
                            Weight += weightedInt.Weight;
                        }
                    }
                    else
                    {
                        continue;
                    }
                }
                else if (weightedInt.Weight > 0f)
                {
                    if (ArtifactOfRevolution.ChaosMode.Value)
                    {
                        pickable.Add(i);
                        pickable.Add(i);
                    }
                    pickable.Add(i);
                    Weight += weightedInt.Weight;
                }
            }
            pickable.Shuffle();

            if (ArtifactOfRevolution.ChaosMode.Value)
            {
                return elements[pickable[0]].value;
            }

            int FS = 100;
            int? @int = null;
            while (@int == null)
            {
                FS--;
                if (FS < 0) { break; }
                var r = UnityEngine.Random.Range(0, Weight + 1);
                for (int __ = 0; __ < pickable.Count; __++)
                {
                    var mod = pickable[__];
                    if (r >= 0 && r <= elements[mod].Weight)
                    {
                        @int = mod;
                        break;
                    }
                }
                if (@int != null) { break; }
            }



            return elements[@int ?? 0].value;
        }

        public T SelectByWeight()
        {
            return this.SelectByWeight(null);
        }

        public WeightedType<T>[] elements;
    }
    public class WeightedType<T>
    {
        public WeightedType(T newVal, int weight = 100, RoR2.ExpansionManagement.ExpansionDef expansionDef = null) 
        {
            value = newVal;
            Weight = weight;
            requiredExpansion = expansionDef;
        }
        public T value;
        public int Weight;
        public RoR2.ExpansionManagement.ExpansionDef requiredExpansion = null;
    }

}
