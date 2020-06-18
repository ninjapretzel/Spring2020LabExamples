using UnityEngine;
using System.Collections;
using System.Collections.Generic;

namespace Lab11 {
	/// <summary> A tree of <see cref="BTNode"/>, which any children have a reference to. </summary>
	public class BehaviourTree : MonoBehaviour {

		/// <summary> Current root of the behaviour tree. </summary>
		public BTNode root;
		/// <summary> True if the tree is running, false otherwise. </summary>
		public bool running;
		/// <summary> Should the tree repeatedly execute it's nodes? </summary>
		public bool repeat;
		/// <summary> Coroutine that is currently running the AI behaviour </summary>
		public Coroutine coroutine;
		/// <summary> Dictionary of shared information for AI node objects to use. </summary>
		public Dictionary<string, object> blackboard { get; private set; }

		/// <summary> Helper method to extract data of a given type from the <see cref="blackboard"/>. 
		/// <para> If successful, <paramref name="result"/> is set to the extracted value.</para>
		/// <para> If unsuccessful, <paramref name="result"/> is set to the default value of <typeparamref name="T"/>. </para></summary>
		/// <typeparam name="T"> Type of data to extract </typeparam>
		/// <param name="key"> Key to extract from </param>
		/// <param name="result"> Result location to write extracted value to</param>
		/// <returns> True, if type of data in <see cref="blackboard"/> at <paramref name="key"/> was of type <typeparamref name="T"/>, false otherwise. 
		/// <para> If successful, <paramref name="result"/> is set to the extracted value.</para>
		/// <para> If unsuccessful, <paramref name="result"/> is set to the default value of <typeparamref name="T"/>. </para></returns>
		public bool TryGet<T>(string key, out T result) {
			// Check if blackboard has the requested key 
			if (blackboard.ContainsKey(key)) {
				// If it does, pull out the object at the key
				object o = blackboard[key];
				// Check to see if it is, in fact, of type `T` or a subtype of `T`.
				bool match = (typeof(T).IsAssignableFrom(o.GetType()));
				// If it is, it will successfully cast to T,
				// Otherwise default value (either null or zeros) is assigned.
				result = match ? (T)o : default(T);
				// Return state of m
				return match;
			}
			// If not contained in dictionary, assign default value (either null or zeros)
			result = default(T);
			// and return false.
			return false;
		}

		/// <summary> Current random walk node state. </summary>
		public BTRandomWalkNode walk;
		/// <summary> Current repeater node state. </summary>
		public BTRepeaterNode repeater;
		/// <summary> Current repeatUntilFailure node state. </summary>
		public BTRepeatUntilFailureNode repeatUntilFailure;
		/// <summary> Current sequencer node state. </summary>
		public BTSequencerNode sequencer;
		/// <summary> Current selector node state. </summary>
		public BTSelectorNode selector;
		/// <summary> Node that always fails. </summary>
		public BTFailNode fail;

		/// <summary> Should the <see cref="BTRepeatUntilFailureNode"/> be used? Makes it walk twice, then stop. </summary>
		public bool useRepeatUntilFailure = false;
		/// <summary> Should the <see cref="BTSelectorNode"/> be used? Makes it walk once, then stop. </summary>
		public bool useSelector = false;

		/// <summary> Called by Unity on object load </summary>
		void Awake() {
			// Create a new blackboard and give it some information:
			blackboard = new Dictionary<string, object>();
			blackboard["WorldBounds"] = new Rect(-5, -5, 10, 10);
			running = false;

			// walkNode is serialized by Unity and has no children, 
			// so no reason to create a new one here. just set it's attached tree:
			walk.tree = this;
			// And tell it to pick the next destination so they don't all converge on the origin
			walk.PickNextDestination();

			// ##### All of these these need attached children, so we need create new ones:
			// Demo setup:
			sequencer = new BTSequencerNode(this, walk);
			repeater = new BTRepeaterNode(this, sequencer);

			// Repeat Until Failure setup (walk, walk, fail)
			repeatUntilFailure = new BTRepeatUntilFailureNode(this, new BTSequencerNode(this, walk, walk, fail));
			// Selector setup: (walk, fail, walk, walk, fail)
			selector = new BTSelectorNode(this, walk, fail, walk, walk, fail);

			// use the repeater as the root by default.
			root = repeater;
			// Only override root iwth others if requested.
			if (useRepeatUntilFailure) { root = repeatUntilFailure; }
			if (useSelector) { root = selector; }
		}

		/// <summary> Called by Unity before the object's first Update </summary>
		void Start() {
			Run();
		}

		/// <summary> Called by Unity every frame </summary>
		void Update() {
			if (!running && repeat) {
				running = true;
				StartCoroutine(RunBehaviour());
			}
		}

		/// <summary> Logic to start running or restart the AI behaviour </summary>
		public void Run() {
			if (!running) {
				// Stop if we are currently running
				if (coroutine != null) { StopCoroutine(coroutine); }
				// Reset running to true
				running = true;
				// And start coroutine
				StartCoroutine(RunBehaviour());
			}
		}

		/// <summary> Logic to terminate the AI behaviour </summary>
		public void Stop() {
			if (coroutine != null) { StopCoroutine(coroutine); }
			running = false;
			repeat = false;
		}

		/// <summary> Coroutine function to run the behaviour tree in. </summary>
		/// <returns> Coroutine </returns>
		IEnumerator RunBehaviour() {
			BTNode.Result result = root.Execute();

			while (result == BTNode.Result.Running) {
				yield return null;
				result = root.Execute();
			}

			running = false;
		}

	}
}
