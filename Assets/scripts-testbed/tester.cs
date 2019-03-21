using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Threading;
using System.Threading.Tasks;

public class tester : MonoBehaviour
{
    Queue<string> messageQueue;
    public float speed = 1;
    // Start is called before the first frame update

    void Start()
    {
        messageQueue = new Queue<string>();
    }

    // Update is called once per frame
    void Update()
    {
        while (messageQueue.Count > 0)
        {
            Debug.Log(messageQueue.Dequeue());
        }
        transform.position += Vector3.forward * Time.deltaTime * speed;
        //Task.Run(() =>
        //{
        //    lock (messageQueue)
        //    {
        //        messageQueue.Enqueue("allo");
        //    }
        //});

        //Task.Run(a => aTask("test"));

        //ThreadPool.QueueUserWorkItem(a => aTask("allo"));
    }

    void aTask(string a)
    {
        lock (messageQueue)
        {
            messageQueue.Enqueue(a);

        }
    }
}
