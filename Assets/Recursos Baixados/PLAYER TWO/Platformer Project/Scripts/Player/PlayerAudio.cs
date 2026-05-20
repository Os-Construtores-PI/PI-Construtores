using UnityEngine;

namespace PLAYERTWO.PlatformerProject
{
	[AddComponentMenu("PLAYER TWO/Platformer Project/Player/Player Audio")]
	public class PlayerAudio : MonoBehaviour
	{
		[Header("Voices")]
		public AudioClip[] jump;
		public AudioClip[] hurt;
		public AudioClip[] attack;
		public AudioClip[] lift;
		public AudioClip[] maneuver;

		[Header("Effects")]
		public AudioClip spin;
		public AudioClip pickUp;
		public AudioClip drop;
		public AudioClip airDive;
		public AudioClip stompSpin;
		public AudioClip stompLanding;
		public AudioClip ledgeGrabbing;
		public AudioClip dash;
		public AudioClip startRailGrind;
		public AudioClip railGrind;
		public AudioClip rollCharge;
		public AudioClip rollChargeRelease;
		public AudioClip homingDash;

		[Header("Audio Sources")]
		public AudioSource voicesAudio;
		public AudioSource effectsAudio;
		public AudioSource grindAudio;

		protected Player m_player;

		protected virtual void InitializePlayer() => m_player = GetComponent<Player>();

		protected virtual void InitializeVoicesAudio()
		{
			if (!voicesAudio)
				voicesAudio = gameObject.AddComponent<AudioSource>();
		}

		protected virtual void InitializeEffectsAudio()
		{
			if (!effectsAudio)
				effectsAudio = gameObject.AddComponent<AudioSource>();
		}

		protected virtual void PlayRandom(AudioSource source, AudioClip[] clips)
		{
			if (clips != null && clips.Length > 0)
			{
				var index = Random.Range(0, clips.Length);

				if (clips[index])
					Play(source, clips[index]);
			}
		}

		protected virtual void Play(AudioSource source, AudioClip audio, bool stopPrevious = true)
		{
			if (audio == null)
				return;

			if (stopPrevious)
				source.Stop();

			source.PlayOneShot(audio);
		}

		protected virtual void PlayVoice(AudioClip audio) => Play(voicesAudio, audio, true);

		protected virtual void PlayEffect(AudioClip audio, bool stopPrevious = false) =>
			Play(effectsAudio, audio, stopPrevious);

		protected virtual void InitializeCallbacks()
		{
			m_player.playerEvents.OnJump.AddListener(() => PlayRandom(voicesAudio, jump));
			m_player.playerEvents.OnHurt.AddListener(() => PlayRandom(voicesAudio, hurt));
			m_player.playerEvents.OnThrow.AddListener(() => PlayEffect(drop));
			m_player.playerEvents.OnStompStarted.AddListener(() => PlayEffect(stompSpin));
			m_player.playerEvents.OnStompLanding.AddListener(() => PlayEffect(stompLanding));
			m_player.playerEvents.OnLedgeGrabbed.AddListener(() => PlayEffect(ledgeGrabbing));
			m_player.playerEvents.OnLedgeClimbing.AddListener(() => PlayRandom(voicesAudio, lift));
			m_player.playerEvents.OnBackflip.AddListener(() => PlayRandom(voicesAudio, maneuver));
			m_player.playerEvents.OnDashStarted.AddListener(() => PlayEffect(dash));
			m_player.entityEvents.OnRailsExit.AddListener(() => grindAudio?.Stop());

			m_player.states.AddEnterListener<RollChargePlayerState>(() => PlayEffect(rollCharge));
			m_player.states.AddExitListener<RollChargePlayerState>(
				() => PlayEffect(rollChargeRelease, true)
			);

			m_player.states.AddEnterListener<HomingDashPlayerState>(() =>
			{
				PlayRandom(voicesAudio, jump);
				PlayEffect(homingDash);
			});

			m_player.playerEvents.OnPickUp.AddListener(() =>
			{
				PlayRandom(voicesAudio, lift);
				PlayEffect(pickUp);
			});

			m_player.playerEvents.OnSpin.AddListener(() =>
			{
				PlayRandom(voicesAudio, attack);
				PlayEffect(spin);
			});

			m_player.playerEvents.OnAirDive.AddListener(() =>
			{
				PlayRandom(voicesAudio, attack);
				PlayEffect(airDive);
			});

			m_player.entityEvents.OnRailsEnter.AddListener(() =>
			{
				PlayEffect(startRailGrind);
				grindAudio?.Play();
			});

			LevelPauser.instance?.OnPause.AddListener(Pause);
			LevelPauser.instance?.OnUnpause.AddListener(UnPause);
		}

		public virtual void Pause()
		{
			voicesAudio.Pause();
			effectsAudio.Pause();
			grindAudio.Pause();
		}

		public virtual void UnPause()
		{
			voicesAudio.UnPause();
			effectsAudio.UnPause();
			grindAudio.UnPause();
		}

		protected virtual void Start()
		{
			InitializeVoicesAudio();
			InitializeEffectsAudio();
			InitializePlayer();
			InitializeCallbacks();
		}
	}
}
