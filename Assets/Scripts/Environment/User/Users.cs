using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class Users : Transform2D
{
    public bool useVRMode;
    private User[] users;

    public override void Initializing() {        
        users = GetComponentsInChildren<User>();  

        if(users.Length != 2) throw new System.Exception("there must be 2 'User' class in 'Users' class");

        foreach(var user in users) {
            user.Initializing();
        }

        if(useVRMode) users[1].gameObject.SetActive(false);
        else users[0].gameObject.SetActive(false);
    }

    public User GetActiveUser() {
        foreach(var user in users) {
            if(user.gameObject.activeInHierarchy) {
                return user;
            }
        }

        throw new System.Exception("There are no tracked user");
    }
}
