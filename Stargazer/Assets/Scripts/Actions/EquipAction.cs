﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EquipAction : MonoBehaviour {
	//Holding angle is in degrees
	public float holdingAngle;
	public float holdingDistance;
	public float droppingDistance;

	private Entity ownerEntity;

	void Awake() {
		ownerEntity = GetComponent<Entity>();
    }

	public void OnEquip(Equipment item, Transform transform) {
        //Player special rule: if equip then turn invisible
        if (ownerEntity.gameObject.tag == "Player") {
            foreach (Transform child in item.transform)
            {
                MeshRenderer render = child.GetComponent<MeshRenderer>();
                if (render != null) {
                    render.enabled = false;
                }
            }
        }

        item.ownerEntity = ownerEntity;
        item.transform.parent = transform;
        ownerEntity.equipment = item;

		Rigidbody body = item.GetComponent<Rigidbody>();
        body.isKinematic = true;
        body.useGravity = false;
        body.detectCollisions = false;

        item.transform.localPosition = Calculate.PositionFromAngle(Vector3.zero, this.transform.forward + Vector3.right * this.holdingAngle, this.holdingDistance);
        item.transform.eulerAngles = this.ownerEntity.direction;
        item.transform.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
	}

	public void OnDrop(Equipment item) {
        //Player special rule: if drop then turn visible
        if (ownerEntity.gameObject.tag == "Player") {
            foreach (Transform child in item.transform)
            {
                MeshRenderer render = child.GetComponent<MeshRenderer>();
                if (render != null) {
                    render.enabled = true;
                }
            }
        }

        Vector3 dropDirection = new Vector3(0, this.ownerEntity.direction.y, 0);
        item.transform.position = Calculate.PositionFromAngle(this.ownerEntity.position, dropDirection, this.droppingDistance);
        item.transform.gameObject.layer = LayerMask.NameToLayer("Item");

        Rigidbody body = item.GetComponent<Rigidbody>();
        body.isKinematic = false;
        body.useGravity = true;
        body.detectCollisions = true;

        item.ownerEntity = null;
        item.transform.parent = null;
        ownerEntity.equipment = null;
    }
}
