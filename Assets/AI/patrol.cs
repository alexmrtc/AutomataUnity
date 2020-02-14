using System.Collections;
using System.Collections.Generic;
using UnityEngine;



public class patrol : AI_Agent
{
    Vector3[] waypoints;
    public Transform target;
    public int maxWaypoints = 10;
    int actualWaypoint = 0;

    public float angularVelocity = 1;
    public float angleToGo;
    public float maxAngle;
    int currentWaypoint = 0;
    public float velocity = 2;

    public float halfAngle;
    public float coneDistance;
    public float combatDistance;

    public float totalAngle;
    public float actualAngle;
    public float initAngle;
    public float randAngle;

    public float randState;
    public float randAttack;

    bool orbitAssigned = false;
    public bool orbitToLeft = false;

    public bool combo = false;
    public int attackType;
    float durationOfAttack1 = 0.7f;
    float durationOfAttack2 = 1.2f;
    float durationOfCombo;

    Color color;

    float seconds = 0;
    float secondsToWait = 2;

    void initPositions()
    {
        List<Vector3> waypointsList = new List<Vector3>();
        float anglePartition = 360.0f / (float)maxWaypoints;
        for (int i = 0; i < maxWaypoints; ++i)
        {
            Vector3 v = transform.position + 5 * Vector3.forward * Mathf.Cos(i * anglePartition)
                + 5 * Vector3.right * Mathf.Sin(i * anglePartition);
            waypointsList.Add(v);

        }
        waypoints = waypointsList.ToArray();
    }

    private void OnDrawGizmos()
    {
        Gizmos.color = Color.white;
        if (waypoints.Length > 0)
        {
            for (int i = 0; i < maxWaypoints; i++)
            {
                Gizmos.DrawSphere(waypoints[i], 1.0f);
            }
        }

        Vector3 rightSide = Quaternion.Euler(Vector3.up * halfAngle) * transform.forward * coneDistance;
        Vector3 leftSide = Quaternion.Euler(Vector3.up * -halfAngle) * transform.forward * coneDistance;

        Gizmos.DrawLine(transform.position, transform.position + transform.forward * coneDistance);
        Gizmos.DrawLine(transform.position, transform.position + rightSide);
        Gizmos.DrawLine(transform.position, transform.position + leftSide);

        Gizmos.DrawLine(transform.position + leftSide, transform.position + transform.forward * coneDistance);
        Gizmos.DrawLine(transform.position + rightSide, transform.position + transform.forward * coneDistance);

        Gizmos.color = color;
        Gizmos.DrawSphere(transform.position + (Vector3.up * 2), 1f);
    }

    void idle()
    {
        if (Input.GetKeyDown(KeyCode.A))
        {
            setState(getState("goto"));
        }
    }



    void goToWaypoint()
    {
        color = Color.green;
        rotateTo(waypoints[actualWaypoint]);
        //transform.forward = Vector3.Normalize(new Vector3(0, , 0));
        transform.position += transform.forward * velocity;
        Debug.Log("waypointHey");

        if (Vector3.Distance(target.position, transform.position) < coneDistance &&
            Vector3.Angle(transform.forward, target.transform.position) <= halfAngle)
        {
            setState(getState("gotoplayer"));
        }
        else if (Vector3.Distance(waypoints[actualWaypoint], transform.position) <= 2)
        {
            setState(getState("nextwp"));
        }
    }

    void calculateNextWaypoint()
    {
        color = Color.blue;
        actualWaypoint++;

        if (actualWaypoint >= maxWaypoints)
        {
            actualWaypoint = (actualWaypoint++) % waypoints.Length;
        }
        setState(getState("goto"));

        //Vector3.SignedAngle();
    }

    void goToPlayer()
    {
        color = Color.red;
        rotateTo(target.position);
        //transform.forward = Vector3.Normalize(new Vector3(0, , 0));
        transform.position += transform.forward * velocity;

        if (Vector3.Distance(target.position, transform.position) > coneDistance &&
            Vector3.Angle(transform.forward, target.transform.position) >= halfAngle)
        {
            setState(getState("goto"));
        }

        if (Vector3.Distance(target.position, transform.position) <= combatDistance)
        {
            setState(getState("idlebattle"));
        }
    }

    void rotateTo(Vector3 position)
    {
        maxAngle = Vector3.SignedAngle(transform.forward, position - transform.position, Vector3.up);
        angleToGo = Mathf.Min(angularVelocity, Mathf.Abs(maxAngle));
        angleToGo *= Mathf.Sign(maxAngle);
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, transform.rotation.eulerAngles.y + angleToGo, transform.rotation.eulerAngles.z);
    }

    void idleBattle()
    {
        if (Vector3.Distance(target.position, transform.position) >= combatDistance)
        {
            setState(getState("gotoplayer"));
        }

        randState = Random.Range(0, 100);
        seconds = 0;

        if (randState >= 60)
        {
            initAngle = transform.rotation.eulerAngles.y;
            actualAngle = transform.rotation.eulerAngles.y;

            int rand = Random.Range(0, 50);
            if (rand > 25)
            {
                orbitToLeft = true;
            }
            else
            {
                orbitToLeft = false;
            }
            setState(getState("orbit"));
        }
        else if (randState >= 20)
        {
            setState(getState("fight"));
        }
        else
        {
            setState(getState("wait"));
        }
    }

    void orbit()
    {
        randAngle = Random.Range(50, 180);
        if (orbitToLeft == true)
        {
            totalAngle = initAngle + randAngle;
            setState(getState("orbitleft"));
        }
        else
        {
            totalAngle = initAngle - randAngle;
            setState(getState("orbitright"));
        }
    }

    void orbitLeft()
    {
        color = Color.cyan;
        float angleToGo = Mathf.Min(angularVelocity - 10, randAngle);
        actualAngle = actualAngle + angleToGo;
        transform.position = combatDistance * transform.forward;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, actualAngle, transform.rotation.eulerAngles.z);
        transform.position = combatDistance * -transform.forward;

        if (actualAngle >= totalAngle)
        {
            setState(getState("idlebattle"));
        }       
    }

    void orbitRight()
    {
        color = Color.yellow;
        float angleToGo = Mathf.Min(angularVelocity - 10, randAngle);
        actualAngle = actualAngle - angleToGo;
        transform.position = combatDistance * transform.forward;
        transform.rotation = Quaternion.Euler(transform.rotation.eulerAngles.x, actualAngle, transform.rotation.eulerAngles.z);
        transform.position = combatDistance * -transform.forward;

        if (actualAngle <= totalAngle)
        {
            setState(getState("idlebattle"));
        }
    }

    void fight()
    {
        randAttack = Random.Range(0, 3);
        switch (randAttack)
        {
            case 0:
                attackType = 0;
                combo = false;
                break;
            case 1:
                combo = true;
                break;
            case 2:
                attackType = 1;
                combo = false;
                break;
        }
        setState(getState("attack"));
    }

    void attack()
    {
        if (combo != true)
        {
            if (attackType == 0)
            {
                color = Color.black;
                setState(getState("attack1"));
            }
            else
            {
                color = Color.white;
                setState(getState("attack2"));
            }
        }
        else
        {
            color = Color.gray;
            setState(getState("attackcombo"));
        }
    }

    void attack1()
    {
        if (seconds < durationOfAttack1)
        {
            seconds += Time.deltaTime;
        }else
        {
            Debug.Log("Attacking with my attack nº "+ attackType);
            setState(getState("idlebattle"));
        }

        if (combo == true)
        {
            seconds = durationOfAttack1;
        }
    }

    void attack2()
    {
        if (seconds < durationOfAttack2)
        {
            seconds += Time.deltaTime;
        }
        else
        {
            Debug.Log("Attacking with my attack nº " + attackType);
            setState(getState("idlebattle"));
        }
    }

    void comboAttack()
    {
        durationOfCombo = durationOfAttack1 + durationOfAttack2;
        if (seconds < durationOfCombo)
        {
            if (seconds < durationOfAttack1)
            {
                setState(getState("attack1"));
            }
            else
            {
                setState(getState("attack2"));
            }
        }
        else
        {
            combo = false;
            setState(getState("idlebattle"));
        }
    }

    void wait()
    {
        if (seconds < secondsToWait)
        {
            seconds += Time.deltaTime;
        }
        else
        {
            setState(getState("idlebattle"));
        }
    }


    // Start is called before the first frame update
    void Start()
    {
        //states["idle"] = idle;
        //states["goto"] = goToWaypoint;
        //states["nextwp"] = calculateNextWaypoint;
        //states["ahora"] = () => { Debug.Log("hey"); };

        initPositions();
        actualWaypoint = 0;
        initState("idle", idle);
        initState("goto", goToWaypoint);
        initState("nextwp", calculateNextWaypoint);
        initState("gotoplayer", goToPlayer);
        initState("idlebattle", idleBattle);
        initState("orbit", orbit);
        initState("orbitleft", orbitLeft);
        initState("orbitright", orbitRight);
        initState("fight", fight);
        initState("attack", attack);
        initState("attack1", attack1);
        initState("attack2", attack2);
        initState("attackcombo", comboAttack);
        initState("wait", wait);


        setState(getState("idle"));
    }

    // Update is called once per frame
    void Update()
    {

    }
}
