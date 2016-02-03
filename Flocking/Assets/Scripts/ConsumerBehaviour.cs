using UnityEngine;
using System.Collections;

public class ConsumerBehaviour : MonoBehaviour
{
	// Update is called once per frame
	void OnCollisionEnter (Collision collision)
    {
	    if(collision.gameObject.CompareTag("Consumable"))
        {
            Destroy(collision.gameObject);
        }
	}
}
