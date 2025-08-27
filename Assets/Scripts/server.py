from mlagents_envs.environment import UnityEnvironment
from mlagents_envs.environment import ActionTuple
import numpy as np
from PIL import Image

def save_observation_as_image(obs, filename="obs_image.png", size=(500, 500)):
    if obs.dtype == np.float32 or obs.max() <= 1.0:
        obs = (obs * 255).astype(np.uint8)

    if obs.ndim == 3 and obs.shape[0] in [1, 3]:  # Convert CHW to HWC
        obs = np.transpose(obs, (1, 2, 0))

    if obs.ndim == 2:
        obs = np.stack([obs]*3, axis=-1)
    elif obs.shape[2] == 1:
        obs = np.repeat(obs, 3, axis=2)

    image = Image.fromarray(obs).resize(size, resample=Image.BILINEAR)
    image.save(filename)
    print(f"Saved resized observation to {filename}")

def concat_save(agent_images, iter):
    if len(agent_images) == 2:
        img1, img2 = agent_images

        # Normalize if needed
        if img1.max() <= 1.0:
            img1 = (img1 * 255).astype(np.uint8)
        if img2.max() <= 1.0:
            img2 = (img2 * 255).astype(np.uint8)

        # Convert CHW to HWC
        if img1.shape[0] in [1, 3]:
            img1 = np.transpose(img1, (1, 2, 0))
        if img2.shape[0] in [1, 3]:
            img2 = np.transpose(img2, (1, 2, 0))

        # Ensure RGB (repeat grayscale if needed)
        if img1.shape[2] == 1:
            img1 = np.repeat(img1, 3, axis=2)
        if img2.shape[2] == 1:
            img2 = np.repeat(img2, 3, axis=2)

        # Resize (optional)
        from PIL import Image
        img1_pil = Image.fromarray(img1).resize((256, 256), resample=Image.BILINEAR)
        img2_pil = Image.fromarray(img2).resize((256, 256), resample=Image.BILINEAR)

        # Concatenate side by side
        concat_img = Image.new('RGB', (img1_pil.width + img2_pil.width, img1_pil.height))
        concat_img.paste(img1_pil, (0, 0))
        concat_img.paste(img2_pil, (img1_pil.width, 0))

        # Save
        concat_img.save(f"obs_concatenated_{iter}.png")
        # print("Saved combined image as obs_concatenated.png")

# Launch Unity environment
env = UnityEnvironment(file_name=None, no_graphics=True)
env.reset()

print(list(env.behavior_specs.keys()))
# Get behavior name and specs
for i in range(len(list(env.behavior_specs.keys()))):
    behavior_name = list(env.behavior_specs.keys())[i]
    spec = env.behavior_specs[behavior_name]

    print(f"Behavior: {behavior_name}")
    print("Observation Specs:")
    for i, obs_spec in enumerate(spec.observation_specs):
        print(f"  Obs {i}: shape={obs_spec.shape}, type={'visual' if len(obs_spec.shape) == 3 else 'vector'}")
    print(f"Action spec: {spec.action_spec}")

# Run a few environment steps
for episode in range(100):
    env.step()
    decision_steps, terminal_steps = env.get_steps(behavior_name)

    num_agents = len(decision_steps)
    print(f"\n--- Episode {episode} ---")
    print(f"Number of agents: {num_agents} Ids: {decision_steps.agent_id}")

    # Prepare actions
    if spec.action_spec.is_discrete():
        actions = np.array([
            [np.random.randint(0, branch) for branch in spec.action_spec.discrete_branches]
            for _ in range(num_agents)
        ], dtype=np.int32)
        action_tuple = ActionTuple(discrete=actions)
        print("Discrete ActionTuple:")
        print(action_tuple.discrete)
    else:
        actions = np.random.uniform(-1, 1, (num_agents, spec.action_spec.continuous_size)).astype(np.float32)
        action_tuple = ActionTuple(continuous=actions)

    # Process each agent's observations
    agent_images = []
    for agent_id in decision_steps.agent_id:
        obs_list = decision_steps[agent_id].obs
        image_obs = None
        vector_obs = None

        for obs in obs_list:
            if len(obs.shape) == 3:  # Likely visual observation
                image_obs = obs
            elif len(obs.shape) == 1:  # Likely vector observation
                vector_obs = obs

        if image_obs is not None:
            # save_observation_as_image(image_obs, filename=f"obs_agent_{agent_id}.png")
            print(f"Agent {agent_id}: Image shape {image_obs.shape}")
            agent_images.append(image_obs)
        else:
            print(f"Agent {agent_id}: No image observation found.")

        if vector_obs is not None:
            print(f"Agent {agent_id}: Vector observation shape {vector_obs.shape}")
        else:
            print(f"Agent {agent_id}: No vector observation found.")

    # Set actions
    env.set_actions(behavior_name, action_tuple)
    # concat_save(agent_images, episode)
    agent_images =[]

env.close()
