using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IRulesetBuilder<StateType, ReferenceType>
{
    public Ruleset<StateType> BuildRuleset(ReferenceType reference);
}
