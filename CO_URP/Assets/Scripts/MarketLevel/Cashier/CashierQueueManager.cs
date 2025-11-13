using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class CashierQueueManager : MonoBehaviour
{
    [System.Serializable]
    public class QueueEntry
    {
        public bool isPlayer;
        public Transform entity;
        public NavMeshAgent agent;
    }

    [SerializeField] Transform servicePoint;
    [SerializeField] Transform[] queuePositions;
    [SerializeField] float serviceDuration = 3f;
    [SerializeField] string playerTag = "Player";

    Queue<QueueEntry> queue = new Queue<QueueEntry>();
    bool servicing;
    float serviceEndTime;

    void Update()
    {
        if (!servicing && queue.Count > 0)
        {
            var front = queue.Peek();
            MoveTo(front, servicePoint.position);
            if (Reached(front, servicePoint.position))
            {
                servicing = true;
                serviceEndTime = Time.time + serviceDuration;
            }
        }
        else if (servicing)
        {
            if (Time.time >= serviceEndTime)
            {
                var front = queue.Dequeue();
                OnServed(front);
                servicing = false;
                ShiftForward();
            }
        }
    }

    void OnTriggerEnter(Collider other)
    {
        var entry = BuildEntry(other);
        if (entry == null) return;
        if (Contains(entry.entity)) return;
        queue.Enqueue(entry);
        AssignPositions();
    }

    void OnTriggerExit(Collider other)
    {
        // If entity leaves, remove from queue
        Remove(other.transform);
        AssignPositions();
    }

    QueueEntry BuildEntry(Collider other)
    {
        if (other.CompareTag(playerTag))
        {
            return new QueueEntry
            {
                isPlayer = true,
                entity = other.transform,
                agent = other.GetComponent<NavMeshAgent>()
            };
        }
        var npc = other.GetComponent<NavMeshAgent>();
        if (npc != null)
        {
            return new QueueEntry { isPlayer = false, entity = other.transform, agent = npc };
        }
        return null;
    }

    bool Contains(Transform t)
    {
        foreach (var e in queue)
        {
            if (e.entity == t) return true;
        }
        return false;
    }

    void Remove(Transform t)
    {
        if (queue.Count == 0) return;
        var list = new List<QueueEntry>(queue);
        list.RemoveAll(e => e.entity == t);
        queue = new Queue<QueueEntry>(list);
    }

    void AssignPositions()
    {
        int i = 0;
        foreach (var e in queue)
        {
            if (i < queuePositions.Length)
            {
                MoveTo(e, queuePositions[i].position);
            }
            i++;
        }
    }

    void ShiftForward()
    {
        AssignPositions();
    }

    void MoveTo(QueueEntry e, Vector3 pos)
    {
        if (e.agent != null && e.agent.isOnNavMesh)
        {
            e.agent.SetDestination(pos);
        }
        else
        {
            e.entity.position = Vector3.MoveTowards(e.entity.position, pos, 2f * Time.deltaTime);
        }
    }

    bool Reached(QueueEntry e, Vector3 pos)
    {
        return Vector3.Distance(e.entity.position, pos) < 0.6f;
    }

    void OnServed(QueueEntry e)
    {
        if (e.isPlayer)
        {
            // TODO: trigger player checkout event (HUD/score)
        }
        else
        {
            // NPC leaves after service
            var exit = e.entity.position + (e.entity.forward * 2f);
            MoveTo(e, exit);
        }
    }
}

