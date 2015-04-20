﻿using UnityEngine;
using System.Collections;

public class HorizontalTank : Unit {
	private enum State {
		Idle,
		AttackUnit,
		AttackBuilding
	};

	public float fireLength;

	public GameObject bullet;

	public float time = 1.0f;
	public float timer = 1.0f;

	private State state = State.Idle;

	private Vector3 firstGoalPosition;
	private Unit goalUnit;
	private Building goalBuilding;

	public override void Go(Vector3 goal) {
		state = State.Idle;
		Map.instance.SetPath(goal, this);
	}

	public override void AttackUnit(Unit unit) {
		state = State.AttackUnit;
		goalUnit = unit;
		firstGoalPosition = goalUnit.transform.position;
		Map.instance.SetPath(goalUnit.transform.position, this);
	}

	public override void AttackBuilding(Building building) {
		state = State.AttackBuilding;
		goalBuilding = building;
		// HACK!
		if (AI.instance.leftFactory == building) {
			Map.instance.SetPath(Torus.instance.GetCortPoint(new Vector3(Mathf.PI - 0.2f, 0.0f, 0.0f)), this);
		}
		if (AI.instance.rightFactory == building) {
			Map.instance.SetPath(Torus.instance.GetCortPoint(new Vector3(Mathf.PI + 0.2f, 0.0f, 0.0f)), this);
		}
	}

	private void Update() {
		bool needGo = false;
		if (state == State.AttackUnit) {
			if (Vector3.Distance(goalUnit.transform.position, cachedTransform.position) < fireLength) {
				if (timer <= 0.0f) {
					var bt = ((GameObject) Instantiate(bullet, cachedTransform.position, Quaternion.identity)).transform;
					bt.up = (goalUnit.transform.position - cachedTransform.position).normalized;
					var b = bt.GetComponent<Bullet>();
					b.goal = goalUnit.transform.position;
					b.unit = goalUnit;
					timer = time;
				}
			}
			else {
				needGo = true;
			}
			if (path == null || Vector3.Distance(goalUnit.transform.position, firstGoalPosition) > BalanceSettings.instance.updateGoalLength) {
				Map.instance.SetPath(goalUnit.transform.position, this);
			}
		}

		if (state == State.AttackBuilding) {
			if (Vector3.Distance(goalBuilding.transform.position, cachedTransform.position) < fireLength) {
				if (timer <= 0.0f) {
					var bt = ((GameObject) Instantiate(bullet, cachedTransform.position, Quaternion.identity)).transform;
					bt.up = (goalBuilding.transform.position - cachedTransform.position).normalized;
					var b = bt.GetComponent<Bullet>();
					b.goal = goalBuilding.transform.position;
					b.building = goalBuilding;
					timer = time;
				}
			}
			else {
				needGo = true;
			}
		}

		if (state == State.Idle) {
			needGo = true;
		}

		if (needGo && path != null && i < path.Length) {
			Vector3 dir = Vector3.zero;

			if (tPosition.x <= Mathf.PI)
				dir.x = Mathf.Abs(path[i].x - 2 * Mathf.PI - tPosition.x) < Mathf.Abs(path[i].x - tPosition.x) ? path[i].x - 2 * Mathf.PI : path[i].x;
			else
				dir.x = Mathf.Abs(path[i].x + 2 * Mathf.PI - tPosition.x) < Mathf.Abs(path[i].x - tPosition.x) ? path[i].x + 2 * Mathf.PI : path[i].x;

			if (tPosition.y <= Mathf.PI)
				dir.y = Mathf.Abs(path[i].y - 2 * Mathf.PI - tPosition.y) < Mathf.Abs(path[i].y - tPosition.y) ? path[i].y - 2 * Mathf.PI : path[i].y;
			else
				dir.y = Mathf.Abs(path[i].y + 2 * Mathf.PI - tPosition.y) < Mathf.Abs(path[i].y - tPosition.y) ? path[i].y + 2 * Mathf.PI : path[i].y;

			tPosition += (dir - tPosition).normalized * speed * Time.deltaTime;

			dir.x -= tPosition.x;
			dir.y -= tPosition.y;
			tPosition.x %= 2 * Mathf.PI;
			tPosition.y %= 2 * Mathf.PI;

			UpdatePosition(torus.GetCortPoint(path[i], height));

			if ((Mathf.Sqrt(dir.x * dir.x + dir.y * dir.y)) < 0.08f) ++i;
			if (i == path.Length) path = null;
		}
	}

}
