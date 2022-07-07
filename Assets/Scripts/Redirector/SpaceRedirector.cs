using System.Collections.Generic;
using System.Collections;
using System;
using UnityEngine;
using UnityEngine.Events;
using System.Linq;

public enum WallMove {
    Shrink,
    Expand
}

public class SpaceRedirector : BaseRedirector
{
    private bool isFirst = true;
    private Dictionary<DistanceType, Dictionary<WallMove, float>> Threshold;

    public void Awake() {
        Threshold = new Dictionary<DistanceType, Dictionary<WallMove, float>>();
        Threshold.Add(DistanceType.Short, new Dictionary<WallMove, float>());
        Threshold.Add(DistanceType.Middle, new Dictionary<WallMove, float>());
        Threshold.Add(DistanceType.Long, new Dictionary<WallMove, float>());

        Threshold[DistanceType.Short].Add(WallMove.Shrink, 0.9f);
        Threshold[DistanceType.Short].Add(WallMove.Expand, 1.15f);
        Threshold[DistanceType.Middle].Add(WallMove.Shrink, 0.87f);
        Threshold[DistanceType.Middle].Add(WallMove.Expand, 1.2f);
        Threshold[DistanceType.Long].Add(WallMove.Shrink, 0.74f);
        Threshold[DistanceType.Long].Add(WallMove.Expand, 1.21f);
    }

    private void Start() {
        User user = users.GetActiveUser();

        user.AddEvent(Behaviour.CompletelyEnter, "NextRoom", (_) => SwitchEnable());
        user.AddEvent(Behaviour.CompletelyEnter, "NextRoom", OvertManipulate);

        StartCoroutine(MyFixedUpdate());
    }

    public bool[] CheckWallVisibleToUser(Room currentRoom, User user) {
        bool[] isVisibleToUser = new bool[4];
        for (int i = 0; i < 4; i++)
            isVisibleToUser[i] = false;

        // 벽이 사용자 시야에 있는지를 판단
        for (int i = 0; i < 4; i++)
        {
            Vector2 vertex1 = currentRoom.GetVertex2D(Utility.mod(i, 4), Space.World);
            Vector2 vertex2 = currentRoom.GetVertex2D(Utility.mod(i + 1, 4), Space.World);

            if(user.IsTargetInUserFov(vertex1, vertex2, 10)) {
                isVisibleToUser[Utility.mod(i, 4)] = true;
            }
        }

        return isVisibleToUser;
    }

    public bool[] CheckWallCenterVisibleToUser(Room currentRoom, User user) {
        bool[] isCenterVisibleToUser = new bool[4];
        for (int i = 0; i < 4; i++)
            isCenterVisibleToUser[i] = false;

                // 벽이 사용자 시야에 있는지를 판단
        for (int i = 0; i < 4; i++)
        {
            Vector2 wallCenter = currentRoom.GetEdge2D(i, Space.World);

            if(user.IsTargetInUserFov(wallCenter)) {
                isCenterVisibleToUser[Utility.mod(i, 4)] = true;
            }
        }

        return isCenterVisibleToUser;
    }

    public Tuple<Vector2, Vector2> GetScaleTranlslate(Room currentRoom, Bound2D realSpace) // v is currentRoom
    {
        Room v = currentRoom;
        Vector2 Scale = new Vector2(v.initSize.x / v.Size.x, v.initSize.y / v.Size.y);
        Vector2 Translate = realSpace.Position - v.Position;

        return new Tuple<Vector2, Vector2>(Scale, Translate);
    }

    private bool[] isWallMoveDone = new bool[4];

    public WallMove CheckWallMove(int wall, float distanceToDest) {
        int wallIndex = Utility.mod(wall, 4);

        if(wallIndex == 0 || wallIndex == 3) {
            if(distanceToDest > 0) return WallMove.Expand;
            else return WallMove.Shrink;
        }
        else {
            if(distanceToDest > 0) return WallMove.Shrink;
            else return WallMove.Expand;
        }
    }

    public DistanceType CheckDistanceType(float distance) {
        if(distance <= 1) return DistanceType.Short;
        else if(distance > 1 && distance <= 2) return DistanceType.Middle;
        else return DistanceType.Long;
    }

    public float GetDistanceFromWallToDestination(Room currentRoom, User user, int wall, Vector2 scale, Vector2 translate) {
        int wallIndex = Utility.mod(wall, 4);
        float distanceToDest = 0;

        switch(wallIndex) {
            case 0:
                distanceToDest = (scale.y - 1) * currentRoom.Size.y / 2 + translate.y;
                break;
            case 1:
                distanceToDest = (1 - scale.x) * currentRoom.Size.x / 2 + translate.x;
                break;
            case 2:
                distanceToDest = (1 - scale.y) * currentRoom.Size.y / 2 + translate.y;
                break;
            case 3:
                distanceToDest = (scale.x - 1) * currentRoom.Size.x / 2 + translate.x;
                break;
            default:
                break;
        }

        return distanceToDest;
    }

    public float GetDistanceFromWallToUser(Room currentRoom, User user, int wall) {
        int wallIndex = Utility.mod(wall, 4);
        float distance = 0;

        switch(wallIndex) {
            case 0:
                distance = (user.Body.Position.y) - currentRoom.GetEdge2D(0, Space.World).y;
                break;
            case 1:
                distance = (user.Body.Position.x) - currentRoom.GetEdge2D(1, Space.World).x;
                break;
            case 2:
                distance = (user.Body.Position.y) - currentRoom.GetEdge2D(2, Space.World).y;
                break;
            case 3:
                distance = (user.Body.Position.x) - currentRoom.GetEdge2D(3, Space.World).x;
                break;
            default:
                break;
        }

        return distance;
    }



    public void Restore(VirtualSpace virtualEnvironment, Room currentRoom, User user, Vector2 scale, Vector2 translate)
    {
        float[] DistWalltoDest = new float[4]; // p,n
        for(int i=0; i<4; i++)
            DistWalltoDest[i] = GetDistanceFromWallToDestination(currentRoom, user, i, scale, translate);

        float[] DistWalltoUser = new float[4]; // short, middle, long
        for(int i=0; i<4; i++) 
            DistWalltoUser[i] = GetDistanceFromWallToUser(currentRoom, user, i);

        bool[] isVisible = CheckWallVisibleToUser(currentRoom, user);
        bool[] isCenterVisible = CheckWallCenterVisibleToUser(currentRoom, user);
        float[] AppliedTranslate = new float[4];

        for(int i=0; i<4; i++) {
            WallMove moveType = CheckWallMove(i, DistWalltoDest[i]);
            DistanceType distanceType = CheckDistanceType(Mathf.Abs(DistWalltoUser[i]));

            float candidate1 = (Mathf.Abs(DistWalltoUser[i]) > 0.4f) ? Mathf.Abs(DistWalltoUser[i]) : 0;
            float candidate2 = 0;
            float candidate3 = Mathf.Abs(DistWalltoDest[i]);

            if(candidate1 < 1.0f) {
                if(i % 2 == 0) candidate2 = Mathf.Abs((Threshold[distanceType][moveType]-1) * currentRoom.Size.y);
                else if(i % 2 != 0) candidate2 = Mathf.Abs((Threshold[distanceType][moveType]-1) * currentRoom.Size.x);
            }
            else {
                candidate2 = Mathf.Abs((Threshold[distanceType][moveType]-1) * DistWalltoUser[i]);
            }

            float destDir = Mathf.Sign(DistWalltoDest[i]);
            float userDir = Mathf.Sign(DistWalltoUser[i]);

            if(destDir * userDir > 0)
                AppliedTranslate[i] = Mathf.Min(candidate1, candidate2, candidate3);
            else
                AppliedTranslate[i] = Mathf.Min(candidate2, candidate3);

            AppliedTranslate[i] *= destDir;

            if(!isVisible[i] && !isWallMoveDone[i]) {
                virtualEnvironment.MoveWall(currentRoom, i, AppliedTranslate[i]);
                isWallMoveDone[i] = true;
            }
            else if(isCenterVisible[i] && isWallMoveDone[i]) {
                isWallMoveDone[i] = false;
            }
        }
    }

    public void Reduce(VirtualSpace virtualEnvironment, Room targetRoom, Room currentRoom, Bound2D realSpace)
    {        
        float xMinDist = realSpace.Min.x - targetRoom.Min.x;
        float xMaxDist = realSpace.Max.x - targetRoom.Max.x;
        float yMinDist = realSpace.Min.y - targetRoom.Min.y;
        float yMaxDist = realSpace.Max.y - targetRoom.Max.y;

        if (xMinDist > 0) // 1벽
        {
            virtualEnvironment.MoveWallWithLimit(targetRoom, 1, xMinDist, currentRoom);
        }
        if (xMaxDist < 0) // 3벽
        {
            virtualEnvironment.MoveWallWithLimit(targetRoom, 3, xMaxDist, currentRoom);
        }
        if (yMinDist > 0) // 2벽
        {
            virtualEnvironment.MoveWallWithLimit(targetRoom, 2, yMinDist, currentRoom);
        }
        if (yMaxDist < 0) // 0벽
        {
            virtualEnvironment.MoveWallWithLimit(targetRoom, 0, yMaxDist, currentRoom);
        }
    }

    public void OvertManipulate(GameObject target) {
        Room targetRoom = target.GetComponent<Room>();
        
        float xMinDist = realSpace.Min.x - targetRoom.Min.x;
        float xMaxDist = realSpace.Max.x - targetRoom.Max.x;
        float yMinDist = realSpace.Min.y - targetRoom.Min.y;
        float yMaxDist = realSpace.Max.y - targetRoom.Max.y;

        if (xMinDist > 0) // 1벽
        {
            virtualEnvironment.MoveWall(targetRoom, 1, xMinDist);
            virtualEnvironment.MoveWall(targetRoom, 3, xMinDist);
        }
        if (xMaxDist < 0) // 3벽
        {
            virtualEnvironment.MoveWall(targetRoom, 3, xMaxDist);
            virtualEnvironment.MoveWall(targetRoom, 1, xMaxDist);
        }
        if (yMinDist > 0) // 2벽
        {
            virtualEnvironment.MoveWall(targetRoom, 2, yMinDist);
            virtualEnvironment.MoveWall(targetRoom, 0, yMinDist);
        }
        if (yMaxDist < 0) // 0벽
        {
            virtualEnvironment.MoveWall(targetRoom, 0, yMaxDist);
            virtualEnvironment.MoveWall(targetRoom, 2, yMaxDist);
        }
    }

    bool NeedAdjust(VirtualSpace virtualEnvironment, Room currentRoom)
    {
        List<Door> connectedDoors = virtualEnvironment.GetConnectedDoors(currentRoom);

        foreach(var door in connectedDoors) {
            if(Mathf.Abs(door.GetThisRoomWrapper(currentRoom).weight - door.GetThisRoomWrapper(currentRoom).originWeight) > 0.01f)
                return true;
        }

        return false;
    }

    void Adjust(VirtualSpace virtualEnvironment, Room currentRoom, User user)
    {
        List<Door> connectedDoors = virtualEnvironment.GetConnectedDoors(currentRoom);

        foreach(var door in connectedDoors) {
            if(!user.IsTargetInUserFov(door.Position))
                virtualEnvironment.MoveDoor(currentRoom, door, door.GetThisRoomWrapper(currentRoom).originWeight);
        }
    }

    public void SwitchEnable() {
        this.enabled = !this.enabled;
        isFirst = !isFirst; // coroutine 자체를 다시 실행시키는 방식으로도 대체 가능
        experiment2.isLocomotionDone = !this.enabled;
    }

    private IEnumerator MyFixedUpdate() {

        yield return new WaitForSeconds(1.0f);

        while(true) {
            User user = users.GetActiveUser();
            Room currentRoom = virtualEnvironment.CurrentRoom;

            // 알고리즘 시작
            Tuple<Vector2, Vector2> st = GetScaleTranlslate(currentRoom, realSpace); 
            Vector2 scale = st.Item1, translate = st.Item2;

            if ((scale - Vector2.one).magnitude > 0.01f || (translate - Vector2.zero).magnitude > 0.01f) // 복원 연산
            {
                Restore(virtualEnvironment, currentRoom, user, scale, translate);
            }
            else if(NeedAdjust(virtualEnvironment, currentRoom)) // 조정 연산
            {
                Adjust(virtualEnvironment, currentRoom, user);
            }
            else // 축소 연산
            {
                if(isFirst) {
                    List<Room> neighborRooms = virtualEnvironment.GetConnectedRooms(currentRoom);
                    foreach (var room in neighborRooms)
                    {
                        if(room == null) continue;
                        if (!room.IsInside(realSpace))
                            Reduce(virtualEnvironment, room, currentRoom, realSpace);
                    }

                    SwitchEnable();
                }
                
            }
            // 알고리즘 끝

            yield return new WaitForFixedUpdate();
        }
    }
}
