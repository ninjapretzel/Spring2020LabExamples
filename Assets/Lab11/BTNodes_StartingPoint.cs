using System.Collections.Generic;
using UnityEngine;
namespace Lab11 {
	/// File contains the BTNodes that were given, along with some slight modifications to them,
	/// For example, marking most of them with serializable attributes to allow them to be viewed from the inspector.

	/// <summary> Base class of an AI behaviour tree node. </summary>
	public abstract class BTNode {
		/// <summary> Results of an execution of a BTNode or its subtypes. </summary>
		public enum Result { Running, Failure, Success, };
		/// <summary> Attached tree. </summary>
		public BehaviourTree tree;

		/// <summary> Empty constructor. Used to make unity complain less. </summary>
		public BTNode() { }
		/// <summary> Constructor to create a new BTNode. </summary>
		/// <param name="t"> Tree to attach to </param>
		public BTNode(BehaviourTree t) {
			tree = t;
		}
	
		/// <summary> Overridable method for AI logic </summary>
		/// <returns> Result of execution. Default just returns <see cref="Result.Failure"/> </returns>
		public virtual Result Execute() { return Result.Failure; }

	}

	/// <summary> Represents a behaviour tree node made up of one or more children. </summary>
	public abstract class BTCompositeNode : BTNode {
		/// <summary> Children nodes contained within this node </summary>
		public List<BTNode> children { get; set; }
		/// <summary> Empty constructor. Used to make unity complain less. </summary>
		public BTCompositeNode() { }
		/// <summary> Constructor that sets an attached tree and provides all children nodes. </summary>
		/// <param name="t"> Tree to attach to </param>
		/// <param name="nodes"> Comma separated list of <see cref="BTNode"/>s, or directly pass in an array. </param>
		public BTCompositeNode(BehaviourTree t, params BTNode[] nodes) : base(t) {
			children = new List<BTNode>(nodes);
		}

	}

	/// <summary> Represents a behaviour tree node that simply decorates a single child node. </summary>
	public abstract class BTDecoratorNode : BTNode {
		/// <summary> Decorated/contained child node </summary>
		public BTNode child { get; set; }
		/// <summary> Empty constructor. Used to make unity complain less. </summary>
		public BTDecoratorNode() { }
		/// <summary> Constructor that sets an attached tree, and provides children node. </summary>
		/// <param name="t"> Tree to attach to </param>
		/// <param name="child"> Single child <see cref="BTNode"/>. </param>
		public BTDecoratorNode(BehaviourTree t, BTNode child) : base(t) {
			this.child = child;
		}

	}

	/// <summary> Represents a behaviour tree node that will run all of its children in sequence. </summary>
	/// <remarks> Marked with <see cref="System.SerializableAttribute"/> to allow Unity to display this <see cref="BTNode"/> in the inspector.</remarks>
	[System.Serializable] public class BTSequencerNode : BTCompositeNode {
		/// <summary> Current position in sequence </summary>
		/// <remarks> Marked with <see cref="UnityEngine.SerializeField"/> attribute to allow unity to serialize this private field. </remarks>
		[SerializeField] private int currentNode = 0;
		/// <summary> Empty constructor. Used to make unity complain less. </summary>
		public BTSequencerNode() { }
		/// <summary> Constructor that sets an attached tree, and provides children node. </summary>
		/// <param name="t"> Tree to attach to </param>
		/// <param name="child"> All child <see cref="BTNode"/>s. </param>
		public BTSequencerNode(BehaviourTree t, params BTNode[] children) : base (t, children) { }
		/// <summary> Execute next child, and return its result. </summary>
		/// <returns> Execution state of next child. </returns>
		public override Result Execute() {
			if (currentNode < children.Count) {
				// Try to execute child node...
				Result result = children[currentNode].Execute();

				// If it's still running, resume it next time.
				if (result == Result.Running) { return result; }
				// If it failed, reset the sequence and return failure.
				if (result == Result.Failure) { currentNode = 0; return result; }

				// Otherwise child succeeded, move to next.
				currentNode++;
				// and if it was not the last one, 
				// move to the next one on the next call
				if (currentNode < children.Count) {
					return Result.Running;
				}

			}
			// If we get down here, we're finished. Reset the sequence and return success. 
			currentNode = 0;
			return Result.Success;
		}

	}

	/// <summary> Behaviour tree node representing a node that always runs its child, and never fails, even if its child fails. </summary>
	/// <remarks> Marked with <see cref="System.SerializableAttribute"/> to allow Unity to display this <see cref="BTNode"/> in the inspector.</remarks>
	[System.Serializable] public class BTRepeaterNode : BTDecoratorNode {
		/// <summary> Empty constructor. Used to make unity complain less. </summary>
		public BTRepeaterNode() { }
		/// <summary> Constructor that sets an attached tree, and provides children node. </summary>
		/// <param name="t"> Tree to attach to </param>
		/// <param name="child"> Single child <see cref="BTNode"/>. </param>
		public BTRepeaterNode(BehaviourTree t, BTNode child) : base (t, child){ }

		/// <summary> Execute child, and always be considered running. </summary>
		/// <returns> <see cref="BTNode.Result.Running"/> Always. </returns>
		public override Result Execute() {
			child.Execute();
			return Result.Running;
		}
	}

	/// <summary> Behaviour tree node representing a random walk behaviour segment </summary>
	/// <remarks> Marked with <see cref="System.SerializableAttribute"/> to allow Unity to display this <see cref="BTNode"/> in the inspector.</remarks>
	[System.Serializable] public class BTRandomWalkNode : BTNode {
		/// <summary> Empty constructor. Used to make unity complain less. </summary>
		public BTRandomWalkNode() { }
		/// <summary> Current target destination </summary>
		public Vector3 nextDestination;
		/// <summary> Current speed. </summary>
		public float speed = 3.0f;
		/// <summary> Constructor that attaches a tree to this behaviour node </summary>
		/// <param name="t"> Tree to attach to. </param>
		public BTRandomWalkNode(BehaviourTree t) : base(t) {
			PickNextDestination();
		}

		/// <summary> Logic for this Behaviour tree node to run. Attempts to move towards the next destination, or pick a new destination. </summary>
		/// <returns> Execution state. </returns>
		public override Result Execute() {
			if (tree.transform.position == nextDestination) {
				if (!PickNextDestination()) { return Result.Failure; }

				return Result.Success;
			} 
			// Up is 'towards camera') in this case...
			tree.transform.LookAt(nextDestination, Vector3.back);
			tree.transform.position = Vector3.MoveTowards(tree.transform.position, nextDestination, Time.deltaTime * speed);
			return Result.Running;
		}

		/// <summary> Logic to pick the next destination. </summary>
		/// <returns> True if a position was picked, false otherwise. </returns>
		public bool PickNextDestination() {
			Rect bounds;
			// C# can infer we are trying to get a `Rect` object out,
			// based on the type of the output variable.
			if (tree.TryGet("WorldBounds", out bounds)) {
				// Pick X/Y values within bounds
				// and Set them into our next destination 
				nextDestination.x = Random.value * bounds.width;
				nextDestination.y = Random.value * bounds.height;
				// Report success
				return true;
			}
			// otherwise report failure.
			return false;
		}
	
	}
}
