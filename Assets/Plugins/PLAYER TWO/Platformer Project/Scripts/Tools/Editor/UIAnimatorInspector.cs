using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

namespace PLAYERTWO.PlatformerProject
{
	[CustomEditor(typeof(UIAnimator))]
	public class UIAnimatorInspector : Editor
	{
		protected UIAnimator m_target;
		protected Animator m_animator;

		protected virtual void AddClipToController(AnimatorController controller, AnimationClip clip)
		{
			AssetDatabase.AddObjectToAsset(clip, controller);
			AssetDatabase.ImportAsset(AssetDatabase.GetAssetPath(clip));
		}

		protected virtual void OnEnable()
		{
			m_target = (UIAnimator)target;
			m_animator = m_target.GetComponent<Animator>();
		}

		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();

			if (!m_animator.runtimeAnimatorController)
			{
				GUILayout.Space(10);

				if (GUILayout.Button("Auto Generate Animation"))
				{
					var path = EditorUtility.SaveFilePanelInProject("New Animation Controller", m_target.gameObject.name, "controller", "");

					if (path.Length == 0)
					{
						return;
					}

					var normalClip = new AnimationClip() { name = m_target.normalTrigger };
					var showClip = new AnimationClip() { name = m_target.showTrigger };
					var hideClip = new AnimationClip() { name = m_target.hideTrigger };

					var controller = AnimatorController.CreateAnimatorControllerAtPath(path);

					AddClipToController(controller, normalClip);
					AddClipToController(controller, showClip);
					AddClipToController(controller, hideClip);

					controller.AddParameter(m_target.normalTrigger, AnimatorControllerParameterType.Trigger);
					controller.AddParameter(m_target.showTrigger, AnimatorControllerParameterType.Trigger);
					controller.AddParameter(m_target.hideTrigger, AnimatorControllerParameterType.Trigger);

					var rootStateMachine = controller.layers[0].stateMachine;

					var normal = rootStateMachine.AddState(m_target.normalTrigger);
					var show = rootStateMachine.AddState(m_target.showTrigger);
					var hide = rootStateMachine.AddState(m_target.hideTrigger);

					normal.motion = normalClip;
					show.motion = showClip;
					hide.motion = hideClip;

					var anyToNormal = rootStateMachine.AddAnyStateTransition(normal);
					anyToNormal.AddCondition(AnimatorConditionMode.If, 0, m_target.normalTrigger);

					var anyToShow = rootStateMachine.AddAnyStateTransition(show);
					anyToShow.AddCondition(AnimatorConditionMode.If, 0, m_target.showTrigger);

					var anyToHide = rootStateMachine.AddAnyStateTransition(hide);
					anyToHide.AddCondition(AnimatorConditionMode.If, 0, m_target.hideTrigger);

					var showToNormal = show.AddTransition(normal);
					showToNormal.hasExitTime = true;

					m_animator.runtimeAnimatorController = controller;
				}
			}
		}
	}
}
