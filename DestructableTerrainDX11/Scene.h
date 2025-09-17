// Application.h
#ifndef SCENE_H
#define SCENE_H

// Includes
#include <chrono>

#include "DXF.h"
#include "DefaultShader.h"
#include "LightMaterialShader.h"
#include "DisplacementShader.h"
#include "NormalMapShader.h"
#include "TextureShader.h"
#include "WaveShader.h"
#include "RenderTexture.h"
#include "MeshInstance.h"
#include "TangentMesh.h"
#include "TransformEditorUI.h"
#include "LightEditorUI.h"
#include "Material.h"
#include "TessellationShader.h"
#include "TessellationQuad.h"
#include "SerializableMesh.h"
#include "UnderwaterEffectShader.h"
#include "TesselatedGerstnerWaveShader.h"
#include "ActiveInstanceSelectorUI.h"
#include "CascadedShadowMaps.h"
#include "HorizontalBlur.h"
#include "MagnifyPixelShader.h"
#include "VerticalBlur.h"
#include <map>
#include <set>
#include <functional>
#include "LuminanceThresholdPass.h"
#include "BloomComposite.h"

#include "BuoyancyComputeShader.h"
#include "RenderItem.h"
#include "PostProcessPassChain.h"
#include "Terrain.h"
#include "TerrainShader.h"
#include "LimitedTimeRenderCollection.h"
#include "insideOutsideShader.h"
#include "ProceduralDestruction.h"
#include "DestructableTerrainPeaks.h"
#include <random>
#include <cmath>
class TerrainTesselationShader;
class btRigidBody;
class btDiscreteDynamicsWorld;
class SkyMapShader;
class SceneJsonSerializer;
//class ActiveInstanceSelectorUI;
struct Ray3D
{
	XMFLOAT3 Origin;
	XMFLOAT3 Direction;
	float tMin = 0;
	float tMax = 0;

	
};


class Scene : public BaseApplication
{
public:
	//---INITIALIZATION METHODS//
#pragma region INITIALIZATION METHODS
	void initPhysics();
	void initMaterials();
	void initRenderCollections();
	void initMeshes();
	void initShaders(HWND hwnd);
	void initSceneComposition(int screenWidth, int screenHeight);
	void fillRenderCollections();
	void autoInsertInRenderCollection(MeshInstance* instance);
	//void manualFillRenderCollections();
	void initCameras();
	void initTextures(int screenWidth, int screenHeight);
	void initLights();
	//void initShadowMap();
	void initCascadedShadowMaps();
#pragma endregion

	//---SCENE VARIABLES---
#pragma region SCENE VARIABLES
	float appTime = 0; // Application Time Since Startup
	clock_t time_start; // Application ticks for previous frame()
	clock_t deltaClockT; // tick difference from previous frame()
	float deltaTime; //float point difference from previous frame()

	//--- RENDER CONTROL PROPERTIES ---
	bool demoWindow;

	//TODO Could be potentially extracted to each of the post-processing effects
	bool enableBlur;
	bool enableBloom = true;
	bool enableMagnify;
	bool enableWaterDistortion;

	//--- FLAGS FOR DIFFERENT RENDERING STAGES OR PASSES ---
	bool isPhysicsPaused;
	bool isRenderTerrainPeak = false;
	bool isRenderDestructablePeak = false;
	bool isRenderSceneToTexture = true;
	bool isPostProcessing = true;

	bool isRenderingWaves = true;
	bool isRenderingSimpleTerrain = false;
	bool isRenderingTessellatedTerrain = true;


	//Debug mode shows all the GUIs 
	bool isDebugMode;
	bool isCameraAttachedToShip;

	DestructibleTerrainPeaks destructableTerrainPeaks;

	// SPECIAL MESH INSTANCES
	// -- USUALLY CONTROLLED BY OTHER PARTS OF THE CODE

	//The instances selected by the user from the UI.
	//That instnace's classses will be updated by the EditorUI methods
	MeshInstance* activeMeshInstance;

	// THE SIMPLE AND TESSELATED TERRAIN MESHES
	MeshInstance* tessellationQuadInstance;
	//MeshInstance* tesselatedTerrainQuadInstance;
	//MeshInstance* TerrainInstance; 

	MeshInstance* skyboxMesh;

	//The buoyant mesh in the scene
	MeshInstance* shipMeshInstance;

	//Debug sphere
	MeshInstance* debugSphere;

	//PHYSICS
	btDiscreteDynamicsWorld* discrete_dynamics_world;
	//The buoyant meshes rigid body. Use to apply the buoyant force from the compute shader
	btRigidBody* shipHullRigidBody;

	//Lights
	std::vector<Light*> lights;
	//Texuter paths in use
	std::map<std::wstring, std::wstring> textureMap;

	//Materials
	std::map<std::string, Material*> materials;

	//Meshes
	std::map<std::string, SerializableMesh> meshes;

	////Mesh instance tree. This are the meshes that are rendered by default.
	std::vector<MeshInstance*> rootMeshInstances;
	std::vector<MeshInstance*> meshInstances;

	//--RENDER COLLECTIONS--

	//Render collection specify a different render strategy for their objects
	std::map<DefaultShader*, RenderItemCollection*> renderCollections;
	LimitedTimeRenderCollection* DestructableComponentsCollections;

	SceneJsonSerializer* serializer;

	#pragma region SHADERS

	//--SHADERS--
	TerrainShader* terrainShader;;
	DefaultShader* defaultShadowShader;
	NormalMapShader* normalAndDisplacementShader;
	SkyMapShader* skyMapShader;
	BuoyancyComputeShader* buoyancyComputeShader;
	TerrainTesselationShader* terrainTesselationShader;
	insideOutsideShader* insideOutsideShaderInstance;

	//TESSELATED
	WaveShader* gerstnerWaveShader;
	TesselatedGerstnerWaveShader* tesselateWaveShader;

	//POST PROCESSING
	TextureShader* textureShader;
	UnderwaterEffectShader* underwaterEffectShader;
	Blur* horizontalBlur;
	Blur* verticalBlur;
	MagnifyPixelShader* magnify;


	LuminanceThresholdPass *thresholdPass;
	BloomComposite *bloomCompositePass;

	//std::vector<XMVECTOR> bouancyForces;

#pragma endregion

	//--POST PROCESSING--
	//Resources for minimap feature
	Camera* minimapCamera;
	OrthoMesh* orthoMesh;
	OrthoMesh* fullScreenOrthoMesh;
	RenderTexture* minimapTexture;
	ID3D11ShaderResourceView* minimapTextureResource;
	ID3D11ShaderResourceView* sceneTextureResource;


#pragma region SCREEN SPACE REFLECTIONS
	//For storing the color values from the first pass of rendering
	ID3D11ShaderResourceView* colourShaderResourceView;
	//For storing the depth values from the first pass of rendering
	ID3D11ShaderResourceView* depthShaderResourceView;
#pragma endregion


	TextureDistortionBuffer distortionBuffer;
	ViggneteMask edgeBlurMask;
	ViggneteMask magnifyMask;

	PostProcessPassChain* passChain;
	//Displacement Value
	//Displacment of sphere in the scene
	float displacementValue;

	//CASCADED SHADOW MAPS
	bool useShadowMap = false;
	CascadedShadowMaps* cascadedShadowMaps;

	//Is camera movement allowed or not
	bool activeCameraMovement = true;

	//--WAVE PARAMETERS--
	TesselationFactors tesselationFactors;
	WaveParameters waveParams[3];

	//--FOG PARAMETERSshipRaySetup
	FogParametersType fogParameters;

	//---UI RELATED VARIABLES---
	//Synchronizes what is displayer by ImGui and actual paramters of instnaces' transform
	TransformEditorUI transformEditor;
	LightEditorUI lightEditor;
	//Updates the activeMeshInstance to what the suer selelcted in the mehs instance tree
	ActiveInstanceSelectorUI* activeInstanceSelectorUI;


	//Test Render TExtures
	RenderTexture* sceneRT;
	RenderTexture* effectRT;
	RenderTexture* bloomTextureOne;
	RenderTexture* bloomTextureTwo;

	ViggneteMask bloomBlurMask;

	XMFLOAT4 playerPos;

#pragma endregion

	Scene();
	~Scene();
	void tickPhysicsSimulation();
	void init(HINSTANCE hinstance, HWND hwnd, int screenWidth, int screenHeight, Input* in, bool VSYNC,
	          bool FULL_SCREEN) override;

	void initRasterStates();

	bool frame() override;
	ID3D11Device* getDevice();
	ID3D11DeviceContext* getDeviceContext();

	//Debug Render Buoyancy Points
	void debugRenderBuoyancyForces(XMMATRIX view, XMMATRIX projection);
	//--RENDER PASSES--

	//Post Processing Passes
	void minimapPass();
	void renderTerrain();
	void renderTesselatedTerrain(XMMATRIX view, XMMATRIX projection);

	void setupPostProcessingPass();

	void renderTexture();

	void renderSkyboxPass();
	void doPostProcessingPass(TextureShader* passShader,RenderTexture* out, RenderTexture* in);
	void waterDistortionPass(RenderTexture* out, RenderTexture* in, RenderTexture* renderTexture);

	//Cacaded shadow map pass
	void cascadedShadowMapPass(Light* directionalLight);

	void renderShadowMapToOrthoMesh(ShadowMap* map, OrthoMesh* mesh);

	void renderTesselatedWave(XMMATRIX view, XMMATRIX projection);

	//Render passes for the whole scene(mesh instance tree)
	void renderSceneToTexture(RenderTexture* out, XMMATRIX view, XMMATRIX projection);
	void renderScene(XMMATRIX view, XMMATRIX projection);

	void setupFullSreenPostProcessing(TextureShader* textureShader, RenderTexture* renderTexture);
	void renderOnFullScreenOrtho(TextureShader* textureShader, RenderTexture* renderTexture);

protected:
	bool render() override;

	void renderDefaultTexture(RenderTexture* out);

#pragma region RTIntersection 
	//Testing ray 
	Ray3D Ray;
	Ray3D ShipRay;
	bool isRenderDebugRay = true;
	bool hasRayHit = false;
	int raySamples = 10;
	float debugSphereScale = 1;

	float RegionSampleOffsetX = 10;
	float RegionSampleOffsetY = 10;

	std::vector<XMVECTOR> rayHisPointsLocal;

	int destructibleChunkTargetDim  = 50;
	void renderDebugSamplesAcrossRay(XMMATRIX view, XMMATRIX projection);
	void renderDebugSphereAt(XMMATRIX view, XMMATRIX projection, std::vector<XMVECTOR>& positions,Material* debugMaterial=nullptr);
	//void CalculateWorlsPositionsOfPeakBFS();

#pragma endregion


	bool isRenderingAboveAndBelowPoints = true;

	ProceduralDestruction proceduralDestruction;
	void renderPointsAboveAndBelow(XMMATRIX view, XMMATRIX projection);


#pragma region Ship Controller

	const float steeringTorqueAmount = 100000.f;



	XMVECTOR getDirectionFromInput();
	void applyShipForces();
	// Trying to get the ship to move
	void applyForceToShip(XMVECTOR direction,float mag);
	void steerShip();






#pragma endregion

#pragma region DestructedComponentsPhysics
	//A container for the physics representation of the destructable components
	//TODO synchronous with the limited time render colleciton
	std::map<btRigidBody*, MeshInstance*>  destructablePhysicsComponents;

	//Translate the transform from Bullet 3D to rendering terms
	void applyPhysicsTranform(btRigidBody* body, MeshInstance* renderInstance);

	void visualizePhysicsOfDestructedComponents();
	


	


#pragma endregion






	// --GUI WINDOWS--
#pragma region GUI WINDOWS
	void gui();

	void shipRaySetup();
	XMVECTOR shipCameraRelPosition;
	float shipCameraFixedHeight;
	void shipCameraSetup();
	void guiTerrainGeneration();

	void guiTestingRay();


	void mainGuiWindow();
	void destructableComponentEffectGuiWindow();
	void fogParameterGuiWindow();
	void waveParametersGuiWindow();
	void postProcessingGuiWindow();
	void meshInstanceTreeGuiWindow();
	void buoyancyParametersGuiWindow();
	void screenSpaceReflectionGuiWindow();
#pragma endregion

private:
	Input* input;
	ID3D11RasterizerState* _debugRasterState;
	ID3D11RasterizerState* _rasterizedStateCullOn = nullptr;
	ID3D11RasterizerState* _rasterizedStateCullOff= nullptr;
	//--UTILITY METHODS--
	void changeStateOfCamera();
	//Constructs the ship mesh instance Boundix Box Hull as rigid body
	void constructHullRigidBody();
	void addTexture(std::wstring name, std::wstring fileapth);
	void setupBaseShaderParamters(DefaultShader* baseShader);
	void setupInstanceParameter(DefaultShader* baseShader, MeshInstance* instance);

	void applyTerrainDetail();
	//void destroyDestructableMesh();

	//void calculateClosestRayIntersections();
	void calculateAllRayIntersections();

public:
	void resetResources();
	void setRootInstances();
	//--UTILITY GETTERS
	FPCamera* getCamera();
	TextureManager* getTextureManager();
};

#endif
