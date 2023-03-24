using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UZVR;

public class NpcRoutine: MonoBehaviour
{
    private static int SPEED = 10;
    private GameTime gameTime;
    public List<TestSingleton.Routine> routines;

    void Start()
    {
        gameTime = FindObjectOfType<GameTime>();
    }

    void Update()
    {
        var routine = GetCurrentRoutine();

        if (routine.Equals(default(TestSingleton.Routine)))
            return;

        var waypoint = TestSingleton.world.waypoints
            .FirstOrDefault(item => item.name.ToLower() == routine.waypoint.ToLower());

        var startPosition = gameObject.transform.position;
        gameObject.transform.position = Vector3.MoveTowards(startPosition, waypoint.position, SPEED * Time.deltaTime);
    }

    private TestSingleton.Routine GetCurrentRoutine()
    {
        var curTime = gameTime.getCurrentDateTime();

        var routine = routines.FirstOrDefault(item => (item.start <= curTime && curTime < item.stop));

        return routine;
    }
}
