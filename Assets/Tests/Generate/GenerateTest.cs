using EMP.Cs;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class GenerateTest : MonoBehaviour
{

    void Start()
    {
        Dictionary<string, object> data = 
            new Dictionary<string, object> {
                { "data", new List<Dictionary<string, object>>
                    {
                        new Dictionary<string, object> { { "className", "ClasseEins"} },
                        new Dictionary<string, object> { { "className", "ClasseZwei"} }
                    }
                },
                { "check", "test" }
        };
        Debug.Log(new TestGenerator().Generate(data));
    }


    public class TestGenerator : CsGenerator
    {
        protected override string GetTemplate()
        {
            return
@"
// Kommentar vor dem Block
#block classblock
class #className# {

}
#endblock
// Kommentar nach dem Block
  // #foreach in data with classblock
#if check == True
is true
#else
its else case
#endif
";
        }
    }
}

