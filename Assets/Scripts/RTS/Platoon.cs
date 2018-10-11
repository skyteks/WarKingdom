﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using System.Linq;

[Serializable]
public class Platoon : MonoBehaviour
{
    public enum FormationModes
    {
        Circle,
        Rectangle,
    }

    public FormationModes formationMode;

    public List<Unit> units = new List<Unit>();

    private float formationOffset = 3f;

    private void Start()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].OnDie += UnitDeadHandler;
        }
    }

    void OnDrawGizmosSelected()
    {
        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null)
            {
                Gizmos.color = new Color(.8f, .8f, 1f, 1f);
                Gizmos.DrawCube(units[i].transform.position, new Vector3(1f, .1f, 1f));
            }
        }
    }

    //Executes a command on all Units
    public void ExecuteCommand(AICommand command)
    {
        if (units.Count == 1)
        {
            units[0].ExecuteCommand(command);
            return;
        }
        Vector3 destination = command.destination;
        Vector3 origin = units.Select(unit => unit.transform.position).FindCentroid();
        Quaternion rotation = Quaternion.LookRotation((destination - origin).normalized);
        Vector3[] offsets = GetFormationOffsets(command.destination);
        for (int i = 0; i < offsets.Length; i++) offsets[i] = destination + rotation * offsets[i];
        for (int i = 0; i < units.Count; i++)
        {
            if (units.Count > 1)
            {
                //change the position for the command for each unit
                //so they move to a formation position rather than in the exact same place
                command.destination = offsets[i];
            }

            units[i].ExecuteCommand(command);
        }
    }

    public void AddUnit(Unit unitToAdd)
    {
        unitToAdd.OnDie += UnitDeadHandler;
        units.Add(unitToAdd);
    }

    //Adds an array of Units to the Platoon, and returns the new length
    public int AddUnits(IList<Unit> unitsToAdd)
    {
        for (int i = 0; i < unitsToAdd.Count; i++)
        {
            AddUnit(unitsToAdd[i]);
        }

        return units.Count;
    }

    //Removes an Unit from the Platoon and returns if the operation was successful
    public bool RemoveUnit(Unit unitToRemove)
    {
        bool isThere = units.Contains(unitToRemove);

        if (isThere)
        {
            units.Remove(unitToRemove);
            unitToRemove.OnDie -= UnitDeadHandler;
        }

        return isThere;
    }

    public void Clear()
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].OnDie -= UnitDeadHandler;
        }
        units.Clear();
    }

    //Returns the current position of the units
    public Vector3[] GetCurrentPositions()
    {
        Vector3[] positions = new Vector3[units.Count];

        for (int i = 0; i < units.Count; i++)
        {
            positions[i] = units[i].transform.position;
        }

        return positions;
    }

    //Returns an array of positions to be used to send units into a circular formation
    public Vector3[] GetFormationOffsets(Vector3 formationCenter)
    {
        //TODO: accomodate bigger numbers
        float currentOffset = formationOffset;
        Vector3[] offsets = new Vector3[units.Count];

        switch (formationMode)
        {
            default:
            case FormationModes.Circle:
                float increment = 360f / units.Count;
                for (int i = 0; i < units.Count; i++)
                {
                    float angle = increment * i;
                    offsets[i] = new Vector3(
                        currentOffset * angle.Cos(),
                        0f,
                        currentOffset * angle.Sin()
                    );
                }
                break;
            case FormationModes.Rectangle:
                float root = Mathf.Sqrt(units.Count);
                if (root % 1f == 0f)
                {
                    int i = 0;
                    float half = (root - 1f) * 0.5f * currentOffset;
                    for (int y = 0; y < root && i < units.Count; y++)
                    {
                        for (int x = 0; x < root && i < units.Count; x++, i++)
                        {
                            offsets[i] = new Vector3(
                                x * currentOffset - half,
                                0f,
                                y * currentOffset - half
                            );
                        }
                    }
                }
                else
                {

                }
                break;
        }

        return offsets;
    }

    //Forces the position of the units. Useful in Edit mode only (Play mode would use the NavMeshAgent)
    public void SetPositions(Vector3[] newPositions)
    {
        for (int i = 0; i < units.Count; i++)
        {
            units[i].transform.position = newPositions[i];
        }
    }

    //Returns true if all the Units are dead
    public bool CheckIfAllDead()
    {
        bool allDead = true;

        for (int i = 0; i < units.Count; i++)
        {
            if (units[i] != null
                && units[i].state != Unit.UnitState.Dead)
            {
                allDead = false;
                break;
            }
        }

        return allDead;
    }

    //Fired when a unit belonging to this Platoon dies
    private void UnitDeadHandler(Unit whoDied)
    {
        RemoveUnit(whoDied); //will also remove the handler
    }

    

    void Update()
    {
        if (Input.GetKeyDown(KeyCode.K)) ExecuteCommand(new AICommand(AICommand.CommandType.GoToAndGuard, Vector3.zero));
    }
}