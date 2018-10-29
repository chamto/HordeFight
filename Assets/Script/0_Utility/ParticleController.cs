using System.Collections;
using System.Collections.Generic;
using UnityEngine;




public class ParticleController : MonoBehaviour 
{
	
	public ParticleSystem _originalParticle = null; 


	private List<ParticleSystem> _particles = new List<ParticleSystem> ();
	private int _nextIndex = 0;

	// Use this for initialization
	void Start () 
	{

		if (null != _originalParticle) 
		{
			_particles.Add (_originalParticle);

			for (int i = 0; i < 10; i++) 
			{
				_particles.Add (this.CloneParticle ());
			}

		}
			
		foreach (ParticleSystem ps in _particles) 
		{
			ps.gameObject.SetActive (false);
		}


	}
	
	// Update is called once per frame
	void Update () 
	{
		
	}


	private ParticleSystem CloneParticle()
	{
		ParticleSystem ps = GameObject.Instantiate<ParticleSystem> (_originalParticle, transform);

		return ps;
	}




	private ParticleSystem NextParticle()
	{
		ParticleSystem ps = _particles[_nextIndex];

		_nextIndex++;
		_nextIndex %= _particles.Count;

		return ps;
	}

	public void PlayDamage(Vector3 worldPos)
	{
		ParticleSystem ps = this.NextParticle ();

		ps.Stop ();

		//ParticleSystem.EmissionModule em = ps.emission;
		//em.rateOverTime = new ParticleSystem.MinMaxCurve (3f);
		//em.rateOverTime = 3f;
		//DebugWide.LogBlue (ps.emission.rateOverTime.constant);

		ps.transform.position = worldPos;
		ps.gameObject.SetActive (true);
		ps.Play ();
	}
}
