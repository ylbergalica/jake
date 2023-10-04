using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GoblinAI : MonoBehaviour, IEnemy {
	public ScriptableObject enemyReference;
	private IGoblin enemyType;
	private Dictionary<string, float> stats;

    private Rigidbody2D rb;
    private float currentHealth;
	private float speed;

	private float timeToReady;

	// Targetting
    private Collider2D[] senseArea;
	private GameObject target;

	void Awake() {
		enemyType = (IGoblin)enemyReference;
		stats = enemyType.GetStats();

		rb = gameObject.GetComponent<Rigidbody2D>();
        speed = stats["speed"] * 1000;
        currentHealth = stats["maxHealth"];
	}

	private void FixedUpdate() {
		senseArea = Physics2D.OverlapCircleAll(new Vector2(transform.position.x, transform.position.y), stats["senseRadius"]);

		// Check if target is still in range
		if (target != null) {
			float distance = Vector3.Distance(target.transform.position, transform.position);

			if (distance > stats["senseRadius"]
				|| target.GetComponent<Player>().currentHealth < 0) {
				target = null;
			}
			else if (distance < 180f
				&& timeToReady + 0.1f < Time.time) {
				// Primary Attack if close enough
				timeToReady = + Time.time + enemyType.GetMoves()[0].GetComponent<Animator>().runtimeAnimatorController.animationClips[0].length;
				enemyType.UsePrimary(gameObject);
			}	
		}

        // Check Sense Area for a Player
        foreach (Collider2D collider in senseArea){
            if (collider.gameObject.tag == "Player") {
                // roses are red, violets are blue, your code is my code too
                Vector3 vectorToTarget = collider.transform.position - transform.position;
                float angle = Mathf.Atan2(vectorToTarget.y, vectorToTarget.x) * Mathf.Rad2Deg - stats["rotationModifier"];
                Quaternion quart = Quaternion.AngleAxis(angle, Vector3.forward);
                transform.rotation = Quaternion.Slerp(transform.rotation, quart, Time.deltaTime * stats["rotationSpeed"]);

                // Chase Player
                rb.AddForce(transform.up * speed * Time.deltaTime);
				target = collider.gameObject;
            }
        }
	}

	private void OnCollisionEnter2D(Collision2D collider) {
        // Do Damage to Player and get KB
        if(collider.gameObject.tag == "Player") {
            Player player = collider.gameObject.GetComponent<Player>();
            player.Hurt(stats["damage"]);

			float distance = Vector3.Distance(collider.transform.position, transform.position);
			float cos = (transform.position.x - collider.transform.position.x) / distance;
			float sin = (transform.position.y - collider.transform.position.y) / distance;
			Vector3 direction = new Vector3(cos, sin, 0);
            rb.AddForce(direction * 20000 * Time.deltaTime, ForceMode2D.Impulse);
        }
    }

    public void Hurt(float damage) {
        currentHealth = Mathf.Clamp(currentHealth - damage, -1, stats["maxHealth"]);

        // Die
        if(currentHealth < 1) {
            Destroy(gameObject);
        }
    }

    public void Heal(float healing) {
        currentHealth = Mathf.Clamp(currentHealth + healing, -1, stats["maxHealth"]);
    }

    public void Knockback(Transform attacker, float kb) {
		float distance = Vector3.Distance(attacker.position, transform.position);
		float cos = (transform.position.x - attacker.position.x) / distance;
		float sin = (transform.position.y - attacker.position.y) / distance;
		Vector3 direction = new Vector3(cos, sin, 0);
        rb.AddForce(direction * kb * 20000 * Time.deltaTime, ForceMode2D.Impulse);
    }

    public void Stun (float seconds) {
        StartCoroutine(IEStun(seconds));
    }

    private IEnumerator IEStun(float seconds) {
        float tempSpeed = speed;
        speed = 0;

        yield return new WaitForSeconds(seconds);
        speed = tempSpeed;
    }
}