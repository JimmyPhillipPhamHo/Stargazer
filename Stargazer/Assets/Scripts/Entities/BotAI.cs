﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BotAI : Entity {

	public GameObject weaponPrefab;
	private Transform centerTransform;
	public GameObject deathExplosion;

	[SerializeField]
	private Transform targetRef;
	private bool targetInLineOfSight;
	private bool targetDetected;
	private bool targetInShootingRange;
	private bool targetCanBeSeen;

	public int detectionRange = 60;
	public int shootingRange = 80;

	private MovementAI movementAI;

	void Start () {
		centerTransform = transform.GetChild(0);
		centerTransform.rotation = transform.rotation;
		this.direction = this.transform.eulerAngles;
		this.position = this.transform.position;
		this.currentSpeed = 0.0f;

		targetDetected = targetCanBeSeen = targetInLineOfSight = targetInShootingRange = false;

		movementAI = GetComponent<MovementAI>();
		Equipment weapon = Instantiate(weaponPrefab, Vector3.zero, Quaternion.Euler(0,0,0)).GetComponent<Equipment>();
		GetComponent<EquipAction>().OnEquip(weapon, this.centerTransform);
	}

	void CheckStatus() {

		Vector3 dir =  targetRef.position - centerTransform.position;
		RaycastHit hitInfoCenter;
		float distApart = .3f;
		float x;
		float z;
		float radians = Mathf.Atan2(dir.z, dir.x);

		RaycastHit hitInfoLeft;
		x = distApart * Mathf.Cos(radians + Mathf.PI / 2);
		z = distApart * Mathf.Sin(radians + Mathf.PI / 2);
		Ray raySpreadLeft = new Ray (position + new Vector3(x, 0, z), dir);
		bool left = Physics.Raycast(raySpreadLeft, out hitInfoLeft);

		RaycastHit hitInfoRight;
		x = distApart * Mathf.Cos(radians - Mathf.PI / 2);
		z = distApart * Mathf.Sin(radians - Mathf.PI / 2);
		Ray raySpreadRight = new Ray (position + new Vector3(x, 0, z), dir);
		bool right = Physics.Raycast(raySpreadRight, out hitInfoRight);

		Ray rayCanBeSeen = new Ray(position, dir);
		bool center = Physics.Raycast(rayCanBeSeen, out hitInfoCenter);

		if (left &&right && center) {
			if (hitInfoCenter.transform.tag == "Player" && 
				hitInfoLeft.transform.tag == "Player" && 
				hitInfoRight.transform.tag == "Player") {
				targetCanBeSeen = true;
			} else {
				targetCanBeSeen = false;
			}
		} else {
			targetCanBeSeen = false;
		}
			

		//Sends a raycast to determine if the player is infront of the enemy.
		Ray rayLineOfSight = new Ray(centerTransform.position, centerTransform.forward);

		if (Physics.Raycast(rayLineOfSight, out hitInfoCenter)) { 
			if (hitInfoCenter.transform.tag == "Player") {
				targetInLineOfSight = true;
				} else {
				targetInLineOfSight = false;
			}
		} else {
			targetInLineOfSight = false;
		}

		//if target is within the spotting range then activate target spotted
		if (Vector3.Distance(targetRef.position, transform.position) >= detectionRange) {
			targetDetected = false;
		} else {
			targetDetected = true;
		}

		if (Vector3.Distance(targetRef.position, transform.position) >= shootingRange) {
			targetInShootingRange = false;
		} else {
			targetInShootingRange = true;
		}
	}

	void AIBehavior() {
		//If target is spotted...
		if (targetDetected) {
			if (movementAI != null) {
				if (!targetInShootingRange || !targetCanBeSeen) {
					movementAI.NavigateTo(targetRef.position);
				} else {
					movementAI.EndNavigation();
				}
			}

			if (targetInLineOfSight)
			{
				this.equipment.OnActivate();
			}

			if (targetCanBeSeen && targetInShootingRange) {
				Quaternion q = Quaternion.LookRotation(targetRef.position - centerTransform.position);

				Quaternion rot = Quaternion.Slerp(centerTransform.rotation, q, this.turnSpeed * Time.deltaTime);

				this.direction = new Vector3(0, rot.eulerAngles.y, 0);
				centerTransform.eulerAngles = new Vector3(rot.eulerAngles.x, centerTransform.eulerAngles.y, rot.eulerAngles.z);
			}
		}
	}

	public override void Death() {
		GameObject explode = Instantiate(deathExplosion, transform.position, transform.rotation) as GameObject;
		explode.transform.localScale = Vector3.one * 2;
		ParticleSystem parts = explode.GetComponent<ParticleSystem>();
		float totalDuration = parts.main.duration + parts.main.startLifetime.constant;
		Destroy(explode, totalDuration);

		Destroy(this.gameObject);
	}

	void Update() {
		if (targetRef != null) {
			CheckStatus();
			AIBehavior();
		}
	}

	private void FixedUpdate() {	
		if (this.currentSpeed != 0.0f) {
			transform.Translate(Vector3.forward * this.currentSpeed * Time.fixedDeltaTime);
			this.position = transform.position;
			centerTransform.rotation = Quaternion.Slerp(centerTransform.rotation, transform.rotation, this.turnSpeed * Time.deltaTime);
		} 
		
		transform.eulerAngles = this.direction;
	}
}
