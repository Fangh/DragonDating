// This is the main file that contains the cloud functions

const {onDocumentWritten} = require("firebase-functions/v2/firestore");
const logger = require("firebase-functions/logger");
const axios = require('axios');
const adminSDK = require("firebase-admin");
const { defineString } = require("firebase-functions/params");

const functionsRegion = "europe-west1"; //pass your region here

//Import data from .env files
const MESHY_API_KEY = defineString('MESHY_API_KEY');

adminSDK.initializeApp();

// This function is triggered when a new user is created
// It will generate a 3D model from the user's profile picture
// Then it will wait for Mesh AI API to process the 3D model
// Then it will download the GLB version
// Then it will upload the GLB version to Firestore in the user folder
exports.generate3DModelForNewUser = onDocumentWritten({region: functionsRegion, document: "dragons/{dragonId}"},async (event) => 
{
    const dragonId = event.params.dragonId;
    logger.log(`Start to Generate a 3D model for : ${dragonId}`);
    logger.log(`First we get the profile picture URL for user : ${dragonId}`);
    const profilePictureURL = await getProfilePictureURL(dragonId);
    logger.log(`Second we start the IA with the task to generate a 3D model for user : ${dragonId} with profile picture URL : ${profilePictureURL}`);
    const taskId = await startImageTo3DTask(profilePictureURL);
    logger.log(`Third we wait for the task to complete with task ID : ${taskId}`);        
    const modelUrl = await waitForTaskCompletion(taskId);
    logger.log(`Fourth we upload the GLB version to Firestore in user folder for user : ${dragonId} with model URL : ${modelUrl}`);        
    await uploadGLBToFirestore(dragonId, modelUrl);
});

// Function to get the user's profile picture URL from Firestore
async function getProfilePictureURL(dragonId) 
{
    return adminSDK.storage().bucket().file(dragonId + "/profilePicture.png").publicUrl();
}

// Function to start the Image to 3D task
async function startImageTo3DTask(profilePictureURL) 
{
    const headers = { Authorization: `Bearer ${MESHY_API_KEY.value()}` };
    const payload = 
    {
        image_url: profilePictureURL,
        enable_pbr: true,
        should_remesh: true,
        should_texture: true
    };

    try 
    {
        const response = await axios.post('https://api.meshy.ai/openapi/v1/image-to-3d', payload, { headers });
        const taskId = response.data.result;
        logger.log(`Task ID: ${taskId}`);
        return taskId;
    } 
    catch (error) 
    {
        logger.error(error);
        throw error;
    }
}

// Function to wait for the task to complete
async function waitForTaskCompletion(taskId) 
{
    const headers = { Authorization: `Bearer ${MESHY_API_KEY.value()}` };
    let taskStatus = 'PENDING';
    let modelUrl = '';

    while (taskStatus !== 'SUCCEEDED') 
    {
        await new Promise(resolve => setTimeout(resolve, 5000)); // Wait for 5 seconds before polling again
        try 
        {
            const taskResponse = await axios.get(`https://api.meshy.ai/openapi/v1/image-to-3d/${taskId}`, { headers });
            taskStatus = taskResponse.data.status;
            logger.log(`Task Status: ${taskStatus}`);
            if (taskStatus === 'SUCCEEDED') 
            {
                modelUrl = taskResponse.data.model_urls.glb;
            }
        } 
        catch (error) 
        {
            logger.error(error);
            throw error;
        }
    }

    return modelUrl;
}

// Function to upload the GLB file to Firestore
async function uploadGLBToFirestore(dragonId, modelUrl) 
{
    try 
    {
        const modelResponse = await axios.get(modelUrl, { responseType: 'arraybuffer' });
        const buffer = Buffer.from(modelResponse.data, 'binary');
        
        const metadata = { contentType: 'model/gltf-binary' };
        await adminSDK.storage().bucket().file(dragonId + "/model.glb").save(buffer, { metadata: metadata });
        logger.log(`Model uploaded to Firestore for user ${dragonId}`);
    } 
    catch (error) 
    {
        logger.error(error);
        throw error;
    }
}

