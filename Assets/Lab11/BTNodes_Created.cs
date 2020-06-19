using UnityEngine;

namespace Lab11 {
	/// <summary> Node to hold logic for repeating execution, but stopping when a child reports failure </summary>
	/// <remarks> Marked with <see cref="System.SerializableAttribute"/> to allow Unity to display this <see cref="BTNode"/> in the inspector.</remarks>
	[System.Serializable] public class BTRepeatUntilFailureNode : BTDecoratorNode {
		/// <summary> Empty constructor. Used to make unity complain less. </summary>
		public BTRepeatUntilFailureNode() { }
		/// <summary> Constructor that sets an attached tree, and provides children node. </summary>
		/// <param name="t"> Tree to attach to </param>
		/// <param name="child"> Single child <see cref="BTNode"/>. </param>
		public BTRepeatUntilFailureNode(BehaviourTree t, BTNode child) : base(t, child) { }

		/// <summary> Execute child, and always be considered running. </summary>
		/// <returns> <see cref="BTNode.Result.Running"/>,
		/// unless the child node reported a <see cref="BTNode.Result.Failure"/> which is propagated. </returns>
		public override Result Execute() {
			var result = child.Execute();
			if (result == Result.Failure) { return Result.Failure; }
		
			return Result.Running;
		}

	}

	/// <summary> Node to hold logic for repeating execution, but stopping when a child reports failure </summary>
	/// <remarks> Marked with <see cref="System.SerializableAttribute"/> to allow Unity to display this <see cref="BTNode"/> in the inspector.</remarks>
	[System.Serializable] public class BTSelectorNode : BTCompositeNode {
		/// <summary> Current position in sequence </summary>
		/// <remarks> Marked with <see cref="UnityEngine.SerializeField"/> attribute to allow unity to serialize this private field. </remarks>
		[SerializeField] private int currentNode = 0;

		/// <summary> Empty constructor. Used to make unity complain less. </summary>
		public BTSelectorNode() { }
		/// <summary> Constructor that sets an attached tree, and provides children node. </summary>
		/// <param name="t"> Tree to attach to </param>
		/// <param name="child"> All child <see cref="BTNode"/>s. </param>
		public BTSelectorNode(BehaviourTree t, params BTNode[] children) : base(t, children) { }

		/// <summary> Logic for selector execution. 
		/// <para> Tries to execute next child, and propagates any <see cref="BTNode.Result.Running"/> or <see cref="BTNode.Result.Success"/> states. </para>
		/// <para> If any <see cref="BTNode.Result.Failure"/>s occur, it tries to execute the next node in sequence, and only propagates the failure if out of children nodes. </para> </summary>
		/// <returns> Result of execution. </returns>
		public override Result Execute() {
			// I loop so that any children that fail immediately don't delay
			// the next child until the next frame... 
			while (currentNode < children.Count) {
				// Try to execute child node...
				Result result = children[currentNode].Execute();

				// If it's still running, propagate the running state and resume it next time.
				if (result == Result.Running) { return result; }

				// If the child node is successful, reset the selector and propagate the success state.
				if (result == Result.Success) { currentNode = 0; return result; }

				// Otherwise child failed, move to next.
				currentNode++;

			}
			// If we get down here, we're finished. Reset the selector and return failure.
			currentNode = 0;
			return Result.Failure;
		}

	}

	/// <summary> Node to hold logic for repeating execution, but stopping when a child reports failure </summary>
	/// <remarks> Marked with <see cref="System.SerializableAttribute"/> to allow Unity to display this <see cref="BTNode"/> in the inspector.</remarks>
	[System.Serializable] public class BTFailNode : BTCompositeNode {
		/// <summary> Empty constructor </summary>
		public BTFailNode() { }
		/// <summary> Always return <see cref="BTNode.Result.Failure"/></summary>
		/// <returns> Always <see cref="BTNode.Result.Failure"/>. </returns>
		public override Result Execute() { return Result.Failure; }

	}
}
