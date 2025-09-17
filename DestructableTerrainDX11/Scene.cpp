#include "Scene.h"
#include  <btBulletDynamicsCommon.h>

#include "DirectXMath.h"
#include "SceneJsonSerializer.h"
#include "SkyMapShader.h"
#include "TerrainTesselationShader.h"
#include "ProceduralMeshA.h"

void Scene::fillRenderCollections()
{
	for (size_t i = 0; i < meshInstances.size(); i++)
	{
		MeshInstance* instance = meshInstances[i];

		if (instance->getMaterial() != nullptr && instance->getMaterial()->name == "Wave")
		{
			if (instance->getSerializableMesh()._type == SerializableMeshType::Plane ||
				instance->getSerializableMesh()._type == SerializableMeshType::TesselationQuad)
			{
				this->renderCollections.at(gerstnerWaveShader)->push_back(instance);
				continue;
			}
		}

		if (instance->getSerializableMesh()._type == SerializableMeshType::TesselationQuad)
		{
			this->tessellationQuadInstance = instance;
		}

		if (instance->getMaterial() == nullptr)
		{
			continue;
		}

		if (!instance->getMaterial()->normalTexture.empty())
		{
			this->renderCollections.at(normalAndDisplacementShader)->push_back(instance);
			continue;
		}

		this->renderCollections.at(defaultShadowShader)->push_back(instance);
	}
}

void Scene::autoInsertInRenderCollection(MeshInstance* instance)
{
	if (instance->getMaterial() != nullptr && instance->getMaterial()->name == "Wave")
	{
		if (instance->getSerializableMesh()._type == SerializableMeshType::Plane ||
			instance->getSerializableMesh()._type == SerializableMeshType::TesselationQuad)
		{
			this->renderCollections.at(gerstnerWaveShader)->push_back(instance);

			return;
		}
	}

	if (instance->getSerializableMesh()._type == SerializableMeshType::TesselationQuad)
	{
		this->tessellationQuadInstance = instance;
	}

	if (instance->getMaterial() == nullptr)
	{
		return;
	}

	if (!instance->getMaterial()->normalTexture.empty())
	{
		this->renderCollections.at(normalAndDisplacementShader)->push_back(instance);
		return;
	}
	this->renderCollections.at(defaultShadowShader)->push_back(instance);
}

Scene::Scene() : lightEditor(LightEditorUI(lights))
{
	//Default parameters debug ray initialization
	Ray.Origin.x = -2.5;
	Ray.Origin.y = 25;
	Ray.Origin.z = 6.5;

	Ray.Direction.x = 0;
	Ray.Direction.y = -1;
	Ray.Direction.z = 1;
	Ray.tMin = 0;
	Ray.tMax = 10;

	ShipRay.Origin.x = 0.5;
	ShipRay.Origin.y = 1.8;
	ShipRay.Direction.x = 0.5;
	ShipRay.tMin = 0;
	ShipRay.tMax = 100;

	waveParams[0].steepness = 0.2f;
	waveParams[0].wavelength = 64;
	waveParams[0].speed = 0.5f;
	waveParams[0].XZdir[0] = 1.0f;
	waveParams[0].XZdir[1] = 0.0f;

	waveParams[1].steepness = 0.180f;
	waveParams[1].wavelength = 31;
	waveParams[1].speed = 0.8f;
	waveParams[1].XZdir[0] = 1.0f;
	waveParams[1].XZdir[1] = -0.3f;

	waveParams[2].steepness = 0.240f;
	waveParams[2].wavelength = 16;
	waveParams[2].speed = 0.9f;
	waveParams[2].XZdir[0] = 1.0f;
	waveParams[2].XZdir[1] = 0.7f;

	distortionBuffer.offsetX = 0.020;
	distortionBuffer.offsetY = 0.020;

	distortionBuffer.sinXFrequency = 20;
	distortionBuffer.sinYFrequency = 20;

	distortionBuffer.colorOverlay = XMFLOAT3(15.0f / 255.0f, 50.0f / 255.0f, 110.0f / 255.0f);

	this->tesselationFactors.edgeTesselationFactor[0] = 4;
	this->tesselationFactors.edgeTesselationFactor[1] = 4;
	this->tesselationFactors.edgeTesselationFactor[2] = 4;
	this->tesselationFactors.edgeTesselationFactor[3] = 4;

	this->tesselationFactors.insideTesselationFactor[0] = 4;
	this->tesselationFactors.insideTesselationFactor[1] = 4;
}

void Scene::constructHullRigidBody()
{
	buoyancyComputeShader->setBuoyantBody(shipMeshInstance);
	shipHullRigidBody = buoyancyComputeShader->constructBuoyantBodyHull();
	if (shipHullRigidBody != nullptr)
		discrete_dynamics_world->addRigidBody(shipHullRigidBody);
}

void Scene::addTexture(std::wstring name, std::wstring fileapth)
{
	this->textureMap.insert({ name, fileapth });
	textureMgr->loadTexture(name, fileapth);
}

void Scene::initPhysics()
{
	// Initialize Bullet physics
	auto collisionConfiguration = new btDefaultCollisionConfiguration();
	auto dispatcher = new btCollisionDispatcher(collisionConfiguration);
	btBroadphaseInterface* overlapingPairCache = new btDbvtBroadphase();
	auto solver = new btSequentialImpulseConstraintSolver;

	discrete_dynamics_world = new btDiscreteDynamicsWorld(dispatcher, overlapingPairCache, solver,
		collisionConfiguration);

	// Set gravity
	discrete_dynamics_world->setGravity(btVector3(0, -9.8f, 0));

	// Create the plane collision shape (static)
	btCollisionShape* groundShape = new btStaticPlaneShape(btVector3(0, 1, 0), 1);

	// Create ground motion state and rigid body
	auto groundMotionState = new btDefaultMotionState();
	btRigidBody::btRigidBodyConstructionInfo groundRigidBodyCI(0, groundMotionState, groundShape);
	auto groundRigidBody = new btRigidBody(groundRigidBodyCI);
	groundRigidBody->translate(btVector3(0, -60, 0));
	discrete_dynamics_world->addRigidBody(groundRigidBody);

	// Clean up
}

void Scene::initMaterials()
{
	const float baseMatCOLOR = 0.5f;
	auto baseMat = new Material();
	baseMat->name = "Base";
	baseMat->ambient = XMFLOAT3(baseMatCOLOR * 0.3, baseMatCOLOR * 0.3, baseMatCOLOR * 0.3);
	baseMat->diffuse = XMFLOAT3(baseMatCOLOR, baseMatCOLOR, baseMatCOLOR);
	baseMat->specular = XMFLOAT3(0.9f, 0.9f, 0.9f);
	baseMat->shininess = 16;
	baseMat->diffuseTexture = L"diffuseBrick";
	this->materials.insert({ baseMat->name, baseMat });

	const float R = 29.0f / 255.0f;
	const float G = 50.0f / 255.0f;
	const float B = 170.0f / 255.0f;

	auto wave = new Material();
	wave->name = "Wave";
	wave->ambient = XMFLOAT3(0.1, 0.1, 0.1);
	wave->diffuse = XMFLOAT3(0.1, 0.1, 0.8);
	wave->specular = XMFLOAT3(1, 1, 1);
	wave->reflectionFactor = 0.015;
	wave->diffuseTexture = L"default";

	// --- DEBUG MATERIALS WITH SIMPEL COLORS --
	XMFLOAT3 debugAmbient = XMFLOAT3(0.1, 0.1, 0.1);
	XMFLOAT3 debugSpecular = XMFLOAT3(1, 1, 1);

	auto blue = new Material();
	blue->name = "Blue";
	blue->ambient = debugAmbient;
	blue->specular = debugSpecular;
	blue->diffuse = XMFLOAT3(0, 0, 1);
	blue->diffuseTexture = L"default";
	blue->normalTexture = L"shipNormal";

	auto green = blue->Copy();
	green->name = "Green";
	green->diffuse = XMFLOAT3(0, 1, 0);
	green->emissive = XMFLOAT3(0, 1, 5);

	auto red = blue->Copy();
	red->name = "Red";
	red->diffuse = XMFLOAT3(1, 0, 0);
	red->emissive = XMFLOAT3(1, 0, 5);

	auto cyan = blue->Copy();
	cyan->name = "Cyan";
	cyan->diffuse = XMFLOAT3(0, 1, 1);
	cyan->emissive = XMFLOAT3(0, 1, 5);

	auto magenta = blue->Copy();
	magenta->name = "Magenta";
	magenta->diffuse = XMFLOAT3(1, 0, 1);

	auto yellow = blue->Copy();
	yellow->name = "Yellow";
	yellow->diffuse = XMFLOAT3(1, 1, 0);

	materials.insert({ blue->name, blue });
	materials.insert({ green->name, green });
	materials.insert({ red->name, red });
	materials.insert({ cyan->name, cyan });
	materials.insert({ magenta->name, magenta });
	materials.insert({ yellow->name, yellow });
	wave->shininess = 16;
	this->materials.insert({ wave->name, wave });

	{
		auto normalMapMaterial = new Material();
		normalMapMaterial->name = "NormalMaterial";
		normalMapMaterial->ambient = XMFLOAT3(baseMatCOLOR * 0.3, baseMatCOLOR * 0.3, baseMatCOLOR * 0.3);
		normalMapMaterial->diffuse = XMFLOAT3(baseMatCOLOR, baseMatCOLOR, baseMatCOLOR);
		normalMapMaterial->specular = XMFLOAT3(0.9f, 0.9f, 0.9f);
		//normalMapMaterial->texturepath = L"res/brickwall.jpg";
		normalMapMaterial->shininess = 16;
		normalMapMaterial->diffuseTexture = L"diffuseBrick";
		normalMapMaterial->normalTexture = L"normalBrick";
		this->materials.insert({ normalMapMaterial->name, normalMapMaterial });
	}

	{
		auto shipMaterial = new Material();
		shipMaterial->name = "ShipMaterial";
		shipMaterial->ambient = XMFLOAT3(0, 0, 0);
		shipMaterial->diffuse = XMFLOAT3(baseMatCOLOR, baseMatCOLOR, baseMatCOLOR);
		shipMaterial->specular = XMFLOAT3(0.9f, 0.9f, 0.9f);
		//normalMapMaterial->texturepath = L"res/brickwall.jpg";
		shipMaterial->shininess = 16;
		shipMaterial->diffuseTexture = L"shipDiffuse";
		//shipMaterial->diffuseTexture = L"default";
		shipMaterial->normalTexture = L"shipNormal";
		this->materials.insert({ shipMaterial->name, shipMaterial });
	}

	{
		auto displacementMapMaterial = new Material(*baseMat);
		displacementMapMaterial->name = "DisplacementMaterial";
		displacementMapMaterial->diffuseTexture = L"diffuseBrick";
		displacementMapMaterial->displacementTexture = L"displacementMap";
		this->materials.insert({ displacementMapMaterial->name, displacementMapMaterial });
	}

	{
		auto rockMaterial = new Material(*baseMat);
		rockMaterial->name = "Rock";
		rockMaterial->diffuseTexture = L"rockDiffuse";
		rockMaterial->normalTexture = L"rockNormal";

		rockMaterial->displacementTexture = L"rockDisplacement";
		this->materials.insert({ rockMaterial->name, rockMaterial });
	}

	{
		auto gravelMaterial = new Material(*baseMat);
		gravelMaterial->name = "Gravel";
		gravelMaterial->diffuseTexture = L"gravelDiffuse";
		gravelMaterial->normalTexture = L"gravelNormal";

		gravelMaterial->displacementTexture = L"gravelDisplacement";
		this->materials.insert({ gravelMaterial->name, gravelMaterial });
	}
}

void Scene::initRenderCollections()
{
	//Setting up render collection. Those are collections that have somethings different for their setup,
	// which in all the cases is additional parameters that need to be passed.
	//Render collection have a shader, a setup funciton and draw all function.
	DestructableComponentsCollections = new LimitedTimeRenderCollection(
		insideOutsideShaderInstance,
		[&](DefaultShader* sh) {
			setupBaseShaderParamters(sh);
		},
		[&](DefaultShader* ls, MeshInstance* instnace)
		{
			setupInstanceParameter(ls, instnace);
			insideOutsideShaderInstance->setInstanceOnTheInside(getDeviceContext());
		});

	auto normalMapRenderCollection = new RenderItemCollection(normalAndDisplacementShader,
		[&](DefaultShader* sh) { setupBaseShaderParamters(sh); },
		[&](DefaultShader* ls, MeshInstance* instnace)
		{
			setupInstanceParameter(ls, instnace);
			normalAndDisplacementShader->setNormalMap(
				renderer->getDeviceContext(),
				textureMgr->getTexture(
					instnace->getMaterial()->normalTexture));
			float displacement = displacementValue;
			if (instnace->getMaterial()->displacementTexture.empty())
				displacement = 0;
			normalAndDisplacementShader->setDiscplacementMap(
				renderer->getDeviceContext(),
				textureMgr->getTexture(
					instnace->getMaterial()->displacementTexture),
				displacement);
			//0.00);
		});

	auto gertsnerWaveRenderCollection = new RenderItemCollection(gerstnerWaveShader,
		[&](DefaultShader* sh)
		{
			setupBaseShaderParamters(sh);
		},
		[&](DefaultShader* ls, MeshInstance* instnace)
		{
			setupInstanceParameter(ls, instnace);
			waveParams[0].time = appTime;
			waveParams[1].time = appTime;
			waveParams[2].time = appTime;
			MultipleWaveBuffer buff;
			buff.waves[0] = waveParams[0];
			buff.waves[1] = waveParams[1];
			buff.waves[2] = waveParams[2];
			gerstnerWaveShader->setWaveParams(
				renderer->getDeviceContext(), buff);
		});

	auto defaultRenderCollection = new RenderItemCollection(defaultShadowShader,
		[&](DefaultShader* sh) { setupBaseShaderParamters(sh); },
		[&](DefaultShader* ls, MeshInstance* instnace)
		{
			setupInstanceParameter(ls, instnace);
		});

	this->renderCollections.insert({ defaultShadowShader, defaultRenderCollection });
	this->renderCollections.insert({ normalMapRenderCollection->shaderToRenderWith, normalMapRenderCollection });
	this->renderCollections.insert({ DestructableComponentsCollections->shaderToRenderWith, DestructableComponentsCollections });
	this->renderCollections.insert({ gertsnerWaveRenderCollection->shaderToRenderWith, gertsnerWaveRenderCollection });
}

void Scene::initMeshes()
{
	{
		SerializableMesh skyboxTemp = SerializableMesh::ShapeMesh("SkyBox", SerializableMeshType::Cube, 100);
		skyboxTemp.CreateMesh(this->getDevice(), this->getDeviceContext());
		meshes.insert({ skyboxTemp.name, skyboxTemp });
	}

	{
		SerializableMesh plane = SerializableMesh::ShapeMesh("Plane", SerializableMeshType::Plane, 100);
		plane.CreateMesh(this->getDevice(), this->getDeviceContext());
		meshes.insert({ plane.name, plane });
	}

	{
		SerializableMesh debugPlane = SerializableMesh::ShapeMesh("DebugPlane", SerializableMeshType::Plane, 10);
		debugPlane.CreateMesh(this->getDevice(), this->getDeviceContext());
		meshes.insert({ debugPlane.name, debugPlane });
	}

	{
		std::string cottageModelStr("res/models/cottage_fbx.fbx");
		SerializableMesh cottage = SerializableMesh::CustomMesh("Cottage", cottageModelStr);
		cottage.CreateMesh(this->getDevice(), this->getDeviceContext());
		meshes.insert({ cottage.name, cottage });
	}

	{

		std::string shipModelStr("res/models/02_barkas.FBX");

		SerializableMesh shipModel = SerializableMesh::CustomMesh("Ship", shipModelStr);
		shipModel.generateTangentMesh = true;
		shipModel.CreateMesh(this->getDevice(), this->getDeviceContext());
		meshes.insert({ shipModel.name, shipModel });
	}
	{
		SerializableMesh sphere = SerializableMesh::ShapeMesh("Sphere", SerializableMeshType::Sphere, 100);
		sphere.CreateMesh(this->getDevice(), this->getDeviceContext());
		meshes.insert({ sphere.name, sphere });
	}


	{
		SerializableMesh debugSphere = SerializableMesh::ShapeMesh("SphereDebug",
			SerializableMeshType::Sphere, 5);
		debugSphere.CreateMesh(this->getDevice(), this->getDeviceContext());
		meshes.insert({ debugSphere.name, debugSphere });
	}


	{
		SerializableMesh cube = SerializableMesh::ShapeMesh("Cube", SerializableMeshType::Cube, 24);
		cube.CreateMesh(this->getDevice(), this->getDeviceContext());
		meshes.insert({ cube.name, cube });
	}


	{
		SerializableMesh tangentSphere = SerializableMesh::ShapeMesh("TangentSphere", SerializableMeshType::Sphere, 100);
		tangentSphere.generateTangentMesh = true;
		tangentSphere.CreateMesh(this->getDevice(), this->getDeviceContext());
		meshes.insert({ tangentSphere.name, tangentSphere });
	}


	{
		SerializableMesh tangentMesh = SerializableMesh::ShapeMesh("TangentMesh", SerializableMeshType::Plane, 5);
		tangentMesh.generateTangentMesh = true;
		tangentMesh.CreateMesh(renderer->getDevice(), renderer->getDeviceContext());
		meshes.insert({ tangentMesh.name, tangentMesh });
	}


	{
		SerializableMesh tesselationQuad = SerializableMesh::ShapeMesh("TesselationQuad",
			SerializableMeshType::TesselationQuad, 150, 250);
		tesselationQuad.CreateMesh(renderer->getDevice(), renderer->getDeviceContext());
		tesselationQuad.GetMesh()->sendData(renderer->getDeviceContext(),
			D3D11_PRIMITIVE_TOPOLOGY_4_CONTROL_POINT_PATCHLIST);
		meshes.insert({ tesselationQuad.name, tesselationQuad });
	}
}

void Scene::initShaders(HWND hwnd)
{
	bloomBlurMask.vInnerRadius = 1;
	bloomBlurMask.vOuterRadius = 0;
	bloomBlurMask.vPower = 2.5;

	ID3D11Device* d = renderer->getDevice();
	skyMapShader						=	new SkyMapShader(d, hwnd);
	defaultShadowShader					=	new DefaultShader(d, hwnd);
	insideOutsideShaderInstance			=	new insideOutsideShader(d, hwnd);
	normalAndDisplacementShader			=	new NormalMapShader(d, hwnd);
	gerstnerWaveShader					=	new WaveShader(d, hwnd,
		L"GerstenVertexShader.cso");
	tesselateWaveShader					=	new TesselatedGerstnerWaveShader(d, hwnd,
		L"TessellationQuadHullShader.cso",
		L"GerstnerWavesDomainShader.cso");

	terrainShader						=	new TerrainShader(d, hwnd);
	terrainTesselationShader			=	new TerrainTesselationShader(d, getDeviceContext(), hwnd,
		L"QuadFBMHullShader.cso",
		L"QuadFBMDomainShader.cso"
	);

	//--- POST PROCESSING SHADERS
	textureShader						=	new TextureShader(d, hwnd);
	underwaterEffectShader				=	new UnderwaterEffectShader(d, hwnd);
	magnify								=	new MagnifyPixelShader(d, hwnd);
	thresholdPass						=	new	LuminanceThresholdPass(d, hwnd);
	bloomCompositePass					=	new BloomComposite(d, hwnd);
	horizontalBlur						=	new Blur(d, hwnd,
		L"HorizontalBlurPixelShader.cso");
	verticalBlur						=	new Blur(d, hwnd,
		L"VerticalBlurPixelShader.cso");

	//--- COMPUTE SHADERS
	buoyancyComputeShader = new BuoyancyComputeShader(d, hwnd, 15, 15);

}

void Scene::initSceneComposition(int screenWidth, int screenHeight)
{
	auto debugTBN = new MeshInstance(meshes.at("TangentMesh"));
	debugTBN->setMaterial(materials.at("Green"));
	debugTBN->transform.setScale(1, 1, 1);
	meshInstances.push_back(debugTBN);
	DestructableComponentsCollections->addRenderItem(debugTBN, 500000);

	auto debugBuoyancy = new MeshInstance(meshes.at("SphereDebug"));
	debugBuoyancy->setMaterial(materials.at("Green"));
	debugBuoyancy->transform.setScale(1, 1, 1);
	debugSphere = debugBuoyancy;

	//The hull of the ship is the primary object updated by the simulation
	auto shipHull = new MeshInstance(meshes.at("Cube"));
	shipHull->setMaterial(materials.at("ShipMaterial"));
	shipHull->transform.setPosition(-5, 20, 0);
	shipHull->transform.setScale(15,2,15);

	shipHull->transform.setRotation(0, 0, -90);

	//The ship mesh itself it a child on top of the hull mesh
	auto shipMeshInstance = new MeshInstance(meshes.at("Ship"));
	shipMeshInstance->setMaterial(materials.at("ShipMaterial"));
	shipMeshInstance->transform.setPosition(2.5 / shipHull->transform.getScale().m128_f32[0], -2, 0);

	shipMeshInstance->transform.setRotation(0, 0, 90);
	float shipScalarScale = 0.15f;
	shipMeshInstance->transform.setScale(
		shipScalarScale * (1.0f / (shipHull->transform.getScale().m128_f32[0])),
		shipScalarScale * (1.0f / (shipHull->transform.getScale().m128_f32[1])),
		shipScalarScale * (1.0f / (shipHull->transform.getScale().m128_f32[2]))
	);
	shipMeshInstance->transform.setParent(&shipHull->transform);
	meshInstances.push_back(shipMeshInstance);
	autoInsertInRenderCollection(shipMeshInstance);

	this->shipMeshInstance = shipHull;

	auto meshInstanceOne = new MeshInstance(meshes.at("TangentSphere"));
	meshInstanceOne->transform.setPosition(-20, 20, 35);
	meshInstanceOne->transform.setScale(15, 15, 15);
	meshInstanceOne->setMaterial(materials.at("Rock"));

	auto meshInstanceTwo = new MeshInstance(meshes.at("TangentSphere"));
	meshInstanceTwo->transform.setPosition(20, 20, 35);
	meshInstanceTwo->transform.setScale(10, 10, 10);
	meshInstanceTwo->setMaterial(materials.at("Gravel"));

	meshInstances.push_back(meshInstanceOne);
	autoInsertInRenderCollection(meshInstanceOne);
	meshInstances.push_back(meshInstanceTwo);
	autoInsertInRenderCollection(meshInstanceTwo);

	//Load plane for normal map testing

	auto normalTestMeshInstance = new MeshInstance(meshes.at("TangentMesh"));

	normalTestMeshInstance->setMaterial(materials.at("NormalMaterial"));
	normalTestMeshInstance->transform.setPosition(0, -15, 70);
	normalTestMeshInstance->transform.setRoll(-90);

	//this->meshes.insert({ "NormalMesh",tMesh });
	this->meshInstances.push_back(normalTestMeshInstance);
	autoInsertInRenderCollection(normalTestMeshInstance);

	tessellationQuadInstance = new MeshInstance(meshes.at("TesselationQuad"));
	tessellationQuadInstance->setMaterial(materials.at("Wave"));
	tessellationQuadInstance->transform.setPosition(0, 0, 0);
	tessellationQuadInstance->transform.setScale(10, 10, 10);

	float len = 200;
	orthoMesh = new OrthoMesh(renderer->getDevice(), renderer->getDeviceContext(), len, len, -2.25 * len,
		sHeight * 0.5 - len * 0.5);
	fullScreenOrthoMesh = new OrthoMesh(renderer->getDevice(), renderer->getDeviceContext(), screenWidth, screenHeight,
		0, 0);


	this->setRootInstances();

	this->activeMeshInstance = meshInstances[1];
}

void Scene::init(HINSTANCE hinstance, HWND hwnd, int screenWidth, int screenHeight, Input* in, bool VSYNC,
	bool FULL_SCREEN)
{
	input = in;
	// Call super/parent init function (required!)
	BaseApplication::init(hinstance, hwnd, screenWidth, screenHeight, in, VSYNC, FULL_SCREEN);

	initRasterStates();

	initPhysics();
	initMaterials();
	initMeshes();
	initShaders(hwnd);
	initRenderCollections();
	initCascadedShadowMaps();
	initLights();
	initTextures(screenWidth, screenHeight);
	initCameras();

	initSceneComposition(screenWidth, screenHeight);

	//Setup scene buoyancy functionality
	constructHullRigidBody();
	shipHullRigidBody->setDamping(0.5, 0.3);

	//Setup destructable terrain 
	destructableTerrainPeaks.initialize(getDevice(), getDeviceContext(),terrainTesselationShader);
	destructableTerrainPeaks.initMeshes();
	destructableTerrainPeaks.initMeshInstances();
	destructableTerrainPeaks.terrainInstance->setMaterial(materials.at("Gravel"));
	destructableTerrainPeaks.tessellatedTerrainQuadInstance->setMaterial(materials.at("Gravel"));
	destructableTerrainPeaks.regenerateSimpleTerrain();

	transformEditor.updateStateOfUI(&activeMeshInstance->transform);
	lightEditor.updateStateOfUI(this->lights);

	this->activeInstanceSelectorUI = new ActiveInstanceSelectorUI();
	activeInstanceSelectorUI->updateStateOfUI(this);

	time_start = clock();

	//Ship camera values
	shipCameraRelPosition.m128_f32[0] = -4;
	shipCameraRelPosition.m128_f32[2] = -0.3;
	shipCameraFixedHeight = 15;

	displacementValue = 0.05f;
	//Init debug raster state

	fogParameters.fogDensity = 0.1f;
	fogParameters.fogColor = XMFLOAT4(1, 0, 0, 1);
	fogParameters.fogEnd = 200.0f;
	fogParameters.padding = 0;

	magnifyMask.vInnerRadius = 0.05;
	magnifyMask.vOuterRadius = 0.150;

	edgeBlurMask.vInnerRadius = 0.3;
	edgeBlurMask.vOuterRadius = 0.6;
}

void Scene::initRasterStates()
{
	CD3D11_RASTERIZER_DESC rasterDesc;
	rasterDesc.CullMode = D3D11_CULL_MODE::D3D11_CULL_NONE;
	rasterDesc.FillMode = D3D11_FILL_MODE::D3D11_FILL_SOLID;
	rasterDesc.ScissorEnable = false;
	rasterDesc.MultisampleEnable = false;
	rasterDesc.AntialiasedLineEnable = false;
	rasterDesc.DepthBias = 0.0f;
	rasterDesc.DepthBiasClamp = 0.0f;
	renderer->getDevice()->CreateRasterizerState(&rasterDesc, &_debugRasterState);

	// (1) Default state: Culling enabled (BACK faces culled)
	D3D11_RASTERIZER_DESC descCullOn = {};
	descCullOn.FillMode = D3D11_FILL_SOLID;
	descCullOn.CullMode = D3D11_CULL_BACK;  // Cull back faces
	descCullOn.FrontCounterClockwise = TRUE; // DX11 defaults to counter-clockwise winding
	getDevice()->CreateRasterizerState(&descCullOn, &_rasterizedStateCullOn);

	// (2) Culling disabled (draw all triangles)
	D3D11_RASTERIZER_DESC descCullOff = {};
	descCullOff.FillMode = D3D11_FILL_SOLID;
	descCullOff.CullMode = D3D11_CULL_NONE;  // Disable culling
	descCullOff.FrontCounterClockwise = TRUE;
	getDevice()->CreateRasterizerState(&descCullOff, &_rasterizedStateCullOff);
}

void Scene::initCameras()
{
	camera->setPosition(0, 20, 0);
	minimapCamera = new Camera();
	minimapCamera->setPosition(50, 25, 50);
	minimapCamera->setRotation(90, 00, 0);
}

void Scene::initTextures(int screenWidth, int screenHeight)
{
	this->textureMap.insert({ L"default", L"default" });

	addTexture(L"res/brick1.dds", L"res/brick1.dds");
	addTexture(L"diffuseBrick", L"res/brickwall.jpg");
	addTexture(L"normalBrick", L"res/brickwall_normal.png");
	addTexture(L"shipDiffuse", L"res/Wood_025_basecolor.jpg");
	addTexture(L"shipNormal", L"res/Wood_025_normal.png");

	//Terrain textures
	addTexture(L"diffuseTerrainSand", L"res/terrain_sand.jpg");
	addTexture(L"diffuseTerrainGrass", L"res/terrain_grass.jpg");
	addTexture(L"diffuseTerrainRock", L"res/terrain_rock.jpg");

	addTexture(L"rockDiffuse", L"res/Rock_047_BaseColor.jpg");
	addTexture(L"rockNormal", L"res/Rock_047_Normal.png");
	addTexture(L"rockDisplacement", L"res/Rock_047_Height.png");

	addTexture(L"gravelDiffuse", L"res/Gravel_001_BaseColor.jpg");
	addTexture(L"gravelNormal", L"res/Gravel_001_Normal.png");
	addTexture(L"gravelDisplacement", L"res/Gravel_001_Height.png");

	sceneRT = new RenderTexture(getDevice(), sWidth, sHeight, 1, 100);
	effectRT = new RenderTexture(getDevice(), sWidth, sHeight, 1, 100);
	bloomTextureOne = new RenderTexture(getDevice(), sWidth, sHeight, 1, 100);
	bloomTextureTwo = new RenderTexture(getDevice(), sWidth, sHeight, 1, 100);

	// Build RenderTexture, this will be our alternative render target.
	minimapTexture = new RenderTexture(renderer->getDevice(), screenWidth, screenHeight, SCREEN_NEAR, SCREEN_DEPTH);

	passChain = new PostProcessPassChain(renderer->getDevice(), sWidth, sHeight, 0.1, 200);
}

void Scene::initLights()
{
	auto lightOne = new Light();
	lightOne->setAmbientColour(0.0f, 0.0f, 0.0f, 1.0f);
	lightOne->setDiffuseColour(0.0f, 0.8f, 0.0f, 1.0f);
	lightOne->setSpecularColour(0.0f, 1.0f, 0.0f, 1.0f);

	lightOne->setPosition(0, 15, 0);
	lightOne->setAttenuationFactors(0.2f, 0.05f, 0);
	lights.push_back(lightOne);
	//lights[0]->setAttenuation(0.2f, 0.1f, 0);

	auto lightTwo = new Light();
	lightTwo->setDiffuseColour(0.7f, 0.7f, 0.0f, 1.0f);
	lightTwo->setSpecularColour(1.0f, 1.0f, 0.0f, 1.0f);
	lightTwo->setDirection(0.71, 0, 0.71);
	lightTwo->innerCutOff = std::cosf(XMConvertToRadians(20.5f));
	lightTwo->outerCutOff = std::cosf(XMConvertToRadians(25.5f));
	lightTwo->setAmbientColour(0, 0, 0, 1);
	lightTwo->setPosition(5, 3, 5);
	lightTwo->setAttenuationFactors(0.2f, 0.01f, 0);
	lights.push_back(lightTwo);

	auto lightThree = new Light();
	lightThree->setAmbientColour(0.15f, 0.15f, 0.15f, 1);
	lightThree->setDiffuseColour(0.9f, 0.9f, 0.9f, 1.0f);
	lightThree->setSpecularColour(1.0f, 1.0f, 1.0f, 1.0f);
	lightThree->setDirection(0, -0.466, 0.885);
	lights.push_back(lightThree);
}

void Scene::initCascadedShadowMaps()
{
	cascadedShadowMaps = new CascadedShadowMaps();
	cascadedShadowMaps->calculateSubFrustrums(renderer->getDevice(), camera, SCREEN_NEAR, SCREEN_NEAR + SCREEN_DEPTH,
		(float)XM_PI / 4.0f, sWidth, sHeight);
}

Scene::~Scene()
{
	// Run base application deconstructor
	BaseApplication::~BaseApplication();
}

void Scene::tickPhysicsSimulation()
{
	deltaClockT = clock() - time_start;
	deltaTime = (float)(deltaClockT) / CLOCKS_PER_SEC;
	time_start = clock();

	MultipleWaveBuffer wave_buffer = {
		waveParams[0],
		waveParams[1],
		waveParams[2],
	};
	buoyancyComputeShader->buoyancyParameters.worldMatrix =
		XMMatrixTranspose(shipMeshInstance->transform.getTransformMatrix());

	//shipHullRigidBody.
	buoyancyComputeShader->computeAndApplyBuoyantForce(shipHullRigidBody, shipMeshInstance, renderer->getDeviceContext(),
		256, 1, 1, wave_buffer);

	if (discrete_dynamics_world != nullptr)
	{
		// Step the simulation, calling update
		discrete_dynamics_world->stepSimulation(deltaTime, 10);

		//Make sure to update rendering transform to visuale the physics changes
		visualizePhysicsOfDestructedComponents();

		//Track the remaining time of the desutructable componenents
		if (DestructableComponentsCollections != nullptr)
			DestructableComponentsCollections->updateTimeStep(deltaTime);

		// Print the cube's position for debugging
		btTransform cubeTransform;
		shipHullRigidBody->getMotionState()->getWorldTransform(cubeTransform);
		auto btVectorOrigin = cubeTransform.getOrigin();

		btQuaternion btQuaternionRot = shipHullRigidBody->getWorldTransform().getRotation();
		XMVECTOR rightHandedQuaternion = XMVectorSet(btQuaternionRot.getX(), btQuaternionRot.getY(),
			btQuaternionRot.getZ(), btQuaternionRot.getW());
		XMVECTOR leftHandedQuaternion = XMVectorSet(-btQuaternionRot.getX(), -btQuaternionRot.getY(),
			-btQuaternionRot.getZ(), btQuaternionRot.getW());
		btQuaternion btQuaternionLeft = btQuaternion(
			leftHandedQuaternion.m128_f32[0], leftHandedQuaternion.m128_f32[1], leftHandedQuaternion.m128_f32[2], leftHandedQuaternion.m128_f32[3]);
		auto leftAngle = btQuaternionRot.getAngle();
		auto leftAxis = btQuaternionRot.getAxis();
		auto rightAngle = btQuaternionLeft.getAngle();
		auto rightAxis = btQuaternionLeft.getAxis();

		shipMeshInstance->transform.setComposeRotationFromQuaternions(true);
		shipMeshInstance->transform.setQuaternion(leftHandedQuaternion.m128_f32[0], leftHandedQuaternion.m128_f32[1],
			leftHandedQuaternion.m128_f32[2], leftHandedQuaternion.m128_f32[3]);
		shipMeshInstance->transform.setPosition(btVectorOrigin.getX(), btVectorOrigin.getY(), btVectorOrigin.getZ());

		btVector3 cubePosition = cubeTransform.getOrigin();
	}
}

bool Scene::frame()
{
	bool result;

	result = BaseApplication::frame();
	if (!result)
	{
		return false;
	}

	// Render the graphics.

	if (input->isKeyDown('P'))
	{
		this->changeStateOfCamera();
	}

	if (input->isKeyDown('G'))
	{
		this->renderer->setWireframeMode(!this->renderer->getWireframeState());
	}

	if (input->isKeyDown('K'))
	{
		destructableTerrainPeaks.regenerateSimpleTerrain();

		//calculateClosestRayIntersections();
	}
	if (input->isKeyDown(' '))
	{
		auto terPtr = (Terrain*)meshes["Terrain"].GetMesh();

		//calculateClosestRayIntersections();
		auto parts = destructableTerrainPeaks.fireProjectileAt(XMLoadFloat3(&Ray.Origin), XMLoadFloat3(&Ray.Direction));
		for each (std::pair<MeshInstance*, btRigidBody*> p in parts)
		{
			p.first->setMaterial(materials.at("Gravel"));
			discrete_dynamics_world->addRigidBody(p.second);
			destructablePhysicsComponents.insert({ p.second, p.first });
			DestructableComponentsCollections->addRenderItem(p.first, 5.0f);
		}

		std::ostringstream oss;
		oss << "RayHitResult_" << destructableTerrainPeaks.simpleTerrainDetail;
		std::string filename = oss.str(); // "RayHitResultNoise_42"

	}
	applyShipForces();

	activeInstanceSelectorUI->updateStateOfUI(this);
	appTime += timer->getTime();

	if (isPhysicsPaused == false)
		tickPhysicsSimulation();

	result = render();
	if (!result)
	{
		return false;
	}

	return true;
}

void Scene::applyShipForces()
{
	applyForceToShip(getDirectionFromInput(), 500);
	steerShip();
}

ID3D11Device* Scene::getDevice()
{
	return renderer->getDevice();
}

ID3D11DeviceContext* Scene::getDeviceContext()
{
	return renderer->getDeviceContext();
}

void Scene::debugRenderBuoyancyForces(XMMATRIX view, XMMATRIX projection)
{
	// Debuging buoyanyc force on the ship
	auto positions = buoyancyComputeShader->positionAlongHull;

	auto forces = buoyancyComputeShader->buoyancyForces;


	XMVECTOR temp;
	std::vector<XMVECTOR> tempPositions;
	for (int i = 0; i < positions.size(); ++i)
	{
		temp = XMVectorSet(positions[i].x, positions[i].y, positions[i].z, 1);
		XMMATRIX mat = this->shipMeshInstance->transform.getTransformMatrix();

		//mat = XMMatrixTranspose(mat);

		temp = XMVector4Transform(temp, mat);
		positions[i].x = temp.m128_f32[0];
		positions[i].y = forces[i];
		positions[i].z = temp.m128_f32[2];
		tempPositions.push_back(XMLoadFloat3(&positions[i]));

	}
	renderDebugSphereAt(view, projection, tempPositions, materials.at("Cyan"));
}

FPCamera* Scene::getCamera() { return this->camera; }

void Scene::minimapPass()
{
	// Set the render target to be the render to texture and clear it
	minimapTexture->setAsRenderTarget(renderer->getDeviceContext());
	minimapTexture->clearRenderTarget(renderer->getDeviceContext(), 0.0f, 0.0f, 1.0f, 1.0f);

	// Get matrices
	minimapCamera->update();
	XMMATRIX viewMatrix = minimapCamera->getViewMatrix();
	float f = (float)sWidth / (float)sHeight;
	float zoom = 100;
	XMMATRIX projectionMatrix = XMMatrixOrthographicLH(zoom, zoom, 0.005, 100);

	XMVECTOR pos = XMLoadFloat3(&camera->getPosition());
	pos = XMVector4Transform(pos, viewMatrix * projectionMatrix);

	//in camera space
	XMStoreFloat4(&playerPos, pos);

	//Screen Space
	if (playerPos.w != 0)
	{
		playerPos.x /= playerPos.w;
		playerPos.y /= playerPos.w;
		playerPos.z /= playerPos.w;
	}

	playerPos.x = (playerPos.x) * 0.5;
	playerPos.y = (1 - playerPos.y * 0.5);

	for (auto renderCollection : this->renderCollections)
	{
		renderCollection.second->SetupShaderAndRenderAllItems(renderer->getDeviceContext());
	}

	minimapTextureResource = minimapTexture->getShaderResourceView();

	// Reset the render target back to the original back buffer and not the render to texture anymore.
	renderer->setBackBufferRenderTarget();
}

void Scene::renderTerrain()
{
	if (isRenderingSimpleTerrain)
	{
		setupBaseShaderParamters(terrainShader);
		setupInstanceParameter(terrainShader, destructableTerrainPeaks.terrainInstance);

		//auto terPtr = (Terrain*)meshes["Terrain"].GetMesh();
		auto terPtr = destructableTerrainPeaks.getSimpleTerrain();
		terrainShader->SetTerrainData(getDeviceContext(), terPtr);
		terPtr->sendData(getDeviceContext());

		terrainShader->render(getDeviceContext(), terPtr->getIndexCount());
	}
	//Render the extract peak

	if (isRenderTerrainPeak && destructableTerrainPeaks.terrainPeak != nullptr)
	{
		setupBaseShaderParamters(defaultShadowShader);
		setupInstanceParameter(defaultShadowShader, destructableTerrainPeaks.terrainInstance);
		destructableTerrainPeaks.terrainPeak->sendData(renderer->getDeviceContext());
		defaultShadowShader->render(getDeviceContext(), destructableTerrainPeaks.terrainPeak->getIndexCount());
	}
	if (isRenderDestructablePeak && destructableTerrainPeaks.destructablePeakMesh != nullptr)
	{
		setupBaseShaderParamters(defaultShadowShader);
		setupInstanceParameter(defaultShadowShader, destructableTerrainPeaks.terrainInstance);
		destructableTerrainPeaks.destructablePeakMesh->sendData(renderer->getDeviceContext());
		defaultShadowShader->render(getDeviceContext(), destructableTerrainPeaks.destructablePeakMesh->getIndexCount());
	}
}

void Scene::renderTesselatedTerrain(XMMATRIX view, XMMATRIX projection)
{
	//Set textures
	terrainTesselationShader->terrainTexture1 =
		textureMgr->getTexture(L"diffuseTerrainSand");
	terrainTesselationShader->terrainTexture2 =
		textureMgr->getTexture(L"diffuseTerrainGrass");
	terrainTesselationShader->terrainTexture3 =
		textureMgr->getTexture(L"diffuseTerrainRock");
	terrainTesselationShader->terrainNormal =
		textureMgr->getTexture(L"gravelNormal");
	terrainTesselationShader->terrainDisplacement =
		textureMgr->getTexture(L"gravelDisplacement");

	auto lProj = cascadedShadowMaps->getOrthoMatrix(0);
	auto lView = cascadedShadowMaps->getViewMatrix(0);

	destructableTerrainPeaks.tessellatedTerrainQuadInstance->getMesh()->sendData(renderer->getDeviceContext(),
		D3D11_PRIMITIVE_TOPOLOGY_4_CONTROL_POINT_PATCHLIST);
	ID3D11ShaderResourceView* texture = nullptr;

	if (!destructableTerrainPeaks.tessellatedTerrainQuadInstance->getMaterial()->diffuseTexture.empty())
	{
		texture = textureMgr->getTexture(destructableTerrainPeaks.tessellatedTerrainQuadInstance->getMaterial()->diffuseTexture);
	}

	terrainTesselationShader->setWorldPositionAndCamera(renderer->getDeviceContext(),
		destructableTerrainPeaks.tessellatedTerrainQuadInstance->transform.getTransformMatrix(),
		camera->getPosition(),
		SCREEN_NEAR, SCREEN_DEPTH
	);

	if (terrainTesselationShader->ssrParameters.useSSR)
	{
		terrainTesselationShader->setSSRColorAndDepthTextures(
			getDeviceContext(),
			colourShaderResourceView,
			depthShaderResourceView
		);
	}

	terrainTesselationShader->setTesselationFactors(
		renderer->getDeviceContext(), tesselationFactors);
	ShadowMappingLights lightsMatriceData{
		cascadedShadowMaps->getViewMatrix(0),
		cascadedShadowMaps->getOrthoMatrix(0),
		cascadedShadowMaps->getViewMatrix(1),
		cascadedShadowMaps->getOrthoMatrix(1),
		cascadedShadowMaps->getViewMatrix(2),
		cascadedShadowMaps->getOrthoMatrix(2),
	};
	terrainTesselationShader->setShaderParameters(renderer->getDeviceContext(),
		view, projection,
		lightsMatriceData,
		lights.data(),
		camera->getPosition(),
		sWidth,
		sHeight
	);
	terrainTesselationShader->setShaderParametersForInstance(
		renderer->getDeviceContext(),
		destructableTerrainPeaks.tessellatedTerrainQuadInstance->transform.getTransformMatrix(),
		destructableTerrainPeaks.tessellatedTerrainQuadInstance->getMaterial(),
		texture
	);
	terrainTesselationShader->sendfBMParams(getDeviceContext(), terrainTesselationShader->fBMParams);

	terrainTesselationShader->sendTerrainParams(getDeviceContext());
	terrainTesselationShader->render(renderer->getDeviceContext(), destructableTerrainPeaks.tessellatedTerrainQuadInstance->getMesh()->getIndexCount());
}

void Scene::setupPostProcessingPass()
{
	renderer->setZBuffer(false);
	//out->setAsRenderTarget(renderer->getDeviceContext());
}

void Scene::renderTexture()
{
}

void Scene::renderSkyboxPass()
{
	//Set default values for the blend state

	D3D11_BLEND_DESC blendDesc = {};
	blendDesc.AlphaToCoverageEnable = FALSE; // Disable alpha-to-coverage
	blendDesc.IndependentBlendEnable = FALSE; // One blend state for all targets

	D3D11_RENDER_TARGET_BLEND_DESC rtBlendDesc = {};
	rtBlendDesc.BlendEnable = FALSE; // Disable blending
	rtBlendDesc.RenderTargetWriteMask = D3D11_COLOR_WRITE_ENABLE_ALL; // Enable writing to all color channels

	blendDesc.RenderTarget[0] = rtBlendDesc;

	ID3D11BlendState* blendState = nullptr;
	getDevice()->CreateBlendState(&blendDesc, &blendState);
	getDeviceContext()->OMSetBlendState(blendState, nullptr, 0xFFFFFFFF); // Bind the blend state
	blendState->Release();

	D3D11_DEPTH_STENCIL_DESC depthStencilDesc = {};
	depthStencilDesc.DepthEnable = TRUE; // Enable depth testing

	// Allow writing to the depth buffer
	depthStencilDesc.DepthWriteMask = D3D11_DEPTH_WRITE_MASK_ALL;
	//The equal here is important as the vertices will always be projected getHeightAt the far plane
	//to simulate infinite distance;
	depthStencilDesc.DepthFunc = D3D11_COMPARISON_LESS_EQUAL;

	depthStencilDesc.StencilEnable = FALSE;
	depthStencilDesc.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
	depthStencilDesc.StencilWriteMask = D3D11_DEFAULT_STENCIL_WRITE_MASK;

	ID3D11DepthStencilState* depthStencilState = nullptr;
	getDevice()->CreateDepthStencilState(&depthStencilDesc, &depthStencilState);
	getDeviceContext()->OMSetDepthStencilState(depthStencilState, 0); // Bind the depth-stencil state
	depthStencilState->Release();

	//Set the debug raster state if needed
	renderer->getDeviceContext()->RSSetState(_debugRasterState);

	XMMATRIX viewMatrix = camera->getViewMatrix();
	XMMATRIX projectionMatrix = renderer->getProjectionMatrix();

	auto ms = new MeshInstance(meshes.at("SkyBox"));
	float scale = 1;
	ms->transform.setPosition(camera->getPosition().x, camera->getPosition().y, camera->getPosition().z);
	ms->transform.setScale(scale, scale, scale);
	ms->getMesh()->sendData(renderer->getDeviceContext());
	skyMapShader->setShaderParameters(renderer->getDeviceContext(), ms->transform.getTransformMatrix(), viewMatrix,
		projectionMatrix);
	skyMapShader->render(getDeviceContext(), ms->getMesh()->getIndexCount());

	depthStencilDesc.DepthFunc = D3D11_COMPARISON_LESS;

	depthStencilDesc.StencilEnable = FALSE;
	depthStencilDesc.StencilReadMask = D3D11_DEFAULT_STENCIL_READ_MASK;
	depthStencilDesc.StencilWriteMask = D3D11_DEFAULT_STENCIL_WRITE_MASK;

	getDevice()->CreateDepthStencilState(&depthStencilDesc, &depthStencilState);
	getDeviceContext()->OMSetDepthStencilState(depthStencilState, 0); // Bind the depth-stencil state

	//Reset previous raster state
	this->renderer->setWireframeMode(this->renderer->getWireframeState());
	return;
}

void Scene::doPostProcessingPass(TextureShader* passShader, RenderTexture* out, RenderTexture* in)
{
	out->setAsRenderTarget(renderer->getDeviceContext());
	passShader->setResolutionParams(renderer->getDeviceContext(), (float)sWidth, (float)sHeight);
	passShader->setIntrinsicParams(renderer->getDeviceContext());
	renderOnFullScreenOrtho(passShader, in);
}

void Scene::cascadedShadowMapPass(Light* directionalLight)
{
	useShadowMap = false;
	XMMATRIX tempView = camera->getViewMatrix();
	XMMATRIX tempProj = renderer->getProjectionMatrix();
	cascadedShadowMaps->updateAll(renderer->getDeviceContext(), camera, directionalLight,
		[&](XMMATRIX lview, XMMATRIX lortho)
		{
			renderTesselatedTerrain(lview, lortho);
			for (auto renderCollection : this->renderCollections)
			{
				//Custom setup to substitue the view and projection matrices that are the camera's by default

				ShadowMappingLights lightsMatriceData{
				};
				renderCollection.first->setShaderParameters(renderer->getDeviceContext(),
					lview,
					lortho,
					lightsMatriceData,
					lights.data(),
					camera->getPosition(),
					sWidth, sHeight);
				fogParameters.camPos = XMFLOAT4(
					camera->getPosition().x, camera->getPosition().y, camera->getPosition().z,
					1);
				renderCollection.first->setFogParameters(
					renderer->getDeviceContext(), fogParameters);

				renderCollection.second->DefaultRenderAll(renderer->getDeviceContext());

				//renderCollection.second->SetupShaderAndRenderAllItems(renderer->getDeviceContext());
			}
		});
	useShadowMap = true;

	renderer->setBackBufferRenderTarget();
}

void Scene::renderShadowMapToOrthoMesh(ShadowMap* map, OrthoMesh* mesh)
{
	XMMATRIX orthoMatrix = renderer->getOrthoMatrix();
	XMMATRIX orthoViewMatrix = camera->getOrthoViewMatrix();

	orthoMesh->sendData(this->renderer->getDeviceContext());
	textureShader->setMatrices(renderer->getDeviceContext(),
		renderer->getWorldMatrix(),
		orthoViewMatrix,
		orthoMatrix);
	textureShader->setTexture(renderer->getDeviceContext(), map->getDepthMapSRV());
	textureShader->render(renderer->getDeviceContext(), orthoMesh->getIndexCount());
}

void Scene::renderSceneToTexture(RenderTexture* out, XMMATRIX view, XMMATRIX projection)
{
	// Set the render target to be the render to texture and clear it
	out->setAsRenderTarget(renderer->getDeviceContext());
	out->clearRenderTarget(renderer->getDeviceContext(), 0.39f, 0.58f, 0.92f, 1.0f);

	renderScene(view, projection);
}

void Scene::renderScene(XMMATRIX view, XMMATRIX projection)
{
	//renderer->setBackBufferRenderTarget();
	// Get matrices

	renderSkyboxPass();
	if (defaultShadowShader->ssrParameters.useSSR)
		defaultShadowShader->setSSRColorAndDepthTextures(
			getDeviceContext(),
			colourShaderResourceView,
			depthShaderResourceView
		);
	renderCollections.at(defaultShadowShader)->SetupShaderAndRenderAllItems(renderer->getDeviceContext());
	for (auto renderCollection : this->renderCollections)
	{
		renderCollection.second->SetupShaderAndRenderAllItems(renderer->getDeviceContext());
	}

	if (isRenderingWaves)
		renderTesselatedWave(view, projection);
	if (isRenderingTessellatedTerrain)
		renderTesselatedTerrain(view, projection);
	renderTerrain();
	if (buoyancyComputeShader->debugVisualizeBuoyantForces)
		debugRenderBuoyancyForces(view, projection);
	if (isRenderDebugRay)
		renderDebugSamplesAcrossRay(view, projection);
	if (isRenderingAboveAndBelowPoints)
		renderPointsAboveAndBelow(view, projection);


	getDeviceContext()->RSSetState(_rasterizedStateCullOff);
	DestructableComponentsCollections->SetupShaderAndRenderAllItems(getDeviceContext());
	getDeviceContext()->RSSetState(_rasterizedStateCullOn);
}

TextureManager* Scene::getTextureManager() { return this->textureMgr; }

void Scene::setRootInstances()
{
	this->rootMeshInstances.clear();
	for (auto meshInstance : this->meshInstances)
	{
		if (meshInstance->transform.parent == nullptr)
		{
			this->rootMeshInstances.push_back(meshInstance);
		}
	}
}

void Scene::resetResources()
{
	for (auto rc : this->renderCollections)
	{
		rc.second->clear();
		delete rc.second;
	}
	this->renderCollections.clear();

	for (auto mesh : meshes)
	{
		delete mesh.second.GetMesh();
	}
	this->meshes.clear();

	this->textureMap.clear();
	delete textureMgr;
	this->textureMgr = new TextureManager(renderer->getDevice(), renderer->getDeviceContext());
	this->textureMap.clear();

	this->rootMeshInstances.clear();
	for (auto instance : this->meshInstances)
	{
		delete instance;
	}
	this->meshInstances.clear();

	for (auto material : materials)
	{
		delete material.second;
	}
	this->materials.clear();

	/*for (auto light : lights)
	{
	delete light;
	}
	this->lights.clear();*/

	this->activeMeshInstance = nullptr;
}

bool Scene::render()
{
	// Generate the view matrix based on the camera's position.
	camera->update();

	// Clear the scene. (default blue colour)
	renderer->beginScene(0.39f, 0.58f, 0.92f, 1.0f);

	renderer->setBackBufferRenderTarget();
	passChain->Reset();

	XMMATRIX viewMatrix;
	XMMATRIX projectionMatrix;
	if (input->isKeyDown('V'))
	{
		projectionMatrix = cascadedShadowMaps->getOrthoMatrix(0);
		viewMatrix = cascadedShadowMaps->getViewMatrix(0);
	}
	else if (input->isKeyDown('B'))
	{
		projectionMatrix = cascadedShadowMaps->getOrthoMatrix(1);
		viewMatrix = cascadedShadowMaps->getViewMatrix(1);
	}
	else if (input->isKeyDown('N'))
	{
		projectionMatrix = cascadedShadowMaps->getOrthoMatrix(2);
		viewMatrix = cascadedShadowMaps->getViewMatrix(2);
	}
	else
	{
		viewMatrix = camera->getViewMatrix();
		projectionMatrix = renderer->getProjectionMatrix();
	}

	defaultShadowShader->ssrParameters.useSSR = false;
	tesselateWaveShader->ssrParameters.useSSR = false;
	//	//	//do an object render pass for color and depth buffer
	renderSceneToTexture(passChain->Out, viewMatrix, projectionMatrix);
	colourShaderResourceView = passChain->Out->getShaderResourceView();
	depthShaderResourceView = passChain->Out->getDepthShaderResourceView();
	passChain->Swap();
	defaultShadowShader->ssrParameters.useSSR = true;
	tesselateWaveShader->ssrParameters.useSSR = true;
	//renderSkyboxPass();
	//  //Do cascaded shadow map pass for the direction light in the scene
	cascadedShadowMapPass(lights[2]);
	//renderer->resetViewport();
	renderer->resetViewport();

	if (isRenderSceneToTexture)
		renderSceneToTexture(passChain->Out, viewMatrix, projectionMatrix);
	else
		renderScene(viewMatrix, projectionMatrix);

	if (isRenderSceneToTexture)
	{
		this->renderer->setZBuffer(false);
		if (isPostProcessing)
		{
			if (enableBloom)
			{
				passChain->Swap();

				thresholdPass->thresholdData.threshold = 1;
				doPostProcessingPass(thresholdPass, bloomTextureOne, passChain->In);
				//thresholdPass(bloomTextureOne, passChain->In);
				//horizontalBlurPass(bloomTextureTwo, bloomTextureOne, bloomBlurMask);

				horizontalBlur->viggnete = bloomBlurMask;
				doPostProcessingPass(horizontalBlur, bloomTextureTwo, bloomTextureOne);

				verticalBlur->viggnete = bloomBlurMask;
				doPostProcessingPass(verticalBlur, bloomTextureOne, bloomTextureTwo);
				//verticalBlurPass(bloomTextureOne, bloomTextureTwo, bloomBlurMask);
				//$bloomCompositePass(passChain->Out, passChain->In, bloomTextureOne, 5, 5);
				bloomCompositePass->bloomData.bloomIntensity = 5;
				bloomCompositePass->bloomData.exposure = 5;
				bloomCompositePass->extractedTexture = bloomTextureOne->getShaderResourceView();
				doPostProcessingPass(bloomCompositePass, passChain->Out, passChain->In);
			}

			if (enableBlur)
			{
				passChain->Swap();
				horizontalBlur->viggnete = edgeBlurMask;
				doPostProcessingPass(horizontalBlur, passChain->Out, passChain->In);

				passChain->Swap();
				verticalBlur->viggnete = edgeBlurMask;
				doPostProcessingPass(verticalBlur, passChain->Out, passChain->In);
			}

			if (enableWaterDistortion)
			{
				passChain->Swap();

				distortionBuffer.time = appTime;
				underwaterEffectShader->setBlurredTexture(renderer->getDeviceContext(), passChain->In->getShaderResourceView());
				underwaterEffectShader->setDistortionParameters(renderer->getDeviceContext(), distortionBuffer, edgeBlurMask);
				doPostProcessingPass(underwaterEffectShader, passChain->Out, passChain->In);
			}

			if (enableMagnify)
			{
				passChain->Swap();
				//magnifyPass(passChain->Out, passChain->In);
				magnify->vignette = magnifyMask;
				doPostProcessingPass(magnify, passChain->Out, passChain->In);
			}
		}
		this->renderer->setZBuffer(true);
		renderer->setBackBufferRenderTarget();
		renderDefaultTexture(passChain->Out);
	}

	//Render GUI
	gui();

	// Present the rendered scene to the screen.
	renderer->endScene();

	return true;
}

void Scene::renderDefaultTexture(RenderTexture* out)
{
	renderer->setZBuffer(false);

	setupFullSreenPostProcessing(textureShader, out);
	fullScreenOrthoMesh->sendData(renderer->getDeviceContext());
	textureShader->render(renderer->getDeviceContext(), fullScreenOrthoMesh->getIndexCount());
	renderer->setZBuffer(true);
}

void Scene::waterDistortionPass(RenderTexture* out, RenderTexture* in, RenderTexture* renderTexture)
{
	distortionBuffer.time = appTime;
	out->setAsRenderTarget(renderer->getDeviceContext());
	setupFullSreenPostProcessing(underwaterEffectShader, in);
	underwaterEffectShader->setBlurredTexture(renderer->getDeviceContext(), renderTexture->getShaderResourceView());
	underwaterEffectShader->setDistortionParameters(renderer->getDeviceContext(), distortionBuffer, edgeBlurMask);
	underwaterEffectShader->render(renderer->getDeviceContext(), fullScreenOrthoMesh->getIndexCount());
}

void Scene::fogParameterGuiWindow()
{
	ImGui::Begin("Fog Parameters");
	//ImGui::DragFloat3("", waveParams[i].XZdir, 0.1f, -1.0f, 1.0f);
	ImGui::ColorEdit4("Fog Color", &fogParameters.fogColor.x);
	ImGui::DragFloat("Fog End", &fogParameters.fogEnd);
	ImGui::DragFloat("Fog Density", &fogParameters.fogDensity);

	ImGui::End();
}

void Scene::meshInstanceTreeGuiWindow()
{
	ImGui::Begin("Mesh Instance Tree");
	activeInstanceSelectorUI->appendToImgui();
	activeInstanceSelectorUI->applyChangesTo(this);
	if (activeInstanceSelectorUI->getRawData().isNew)
	{
		this->transformEditor.updateStateOfUI(&this->activeMeshInstance->transform);
	}

	transformEditor.appendToImgui();
	transformEditor.applyChangesTo(&this->activeMeshInstance->transform);
	ImGui::End();
}

void Scene::screenSpaceReflectionGuiWindow()
{
	ImGui::Begin("Screen Space Reflections");
	ImGui::Checkbox("Use Reflections", &tesselateWaveShader->ssrParameters.useSSR);

	auto waterMar = materials.at("Wave");
	ImGui::DragFloat("Water Reflection Factor", &waterMar->reflectionFactor, 0.0001, 0.0001, 1);
	ImGui::DragFloat("Reflection Ray Length", &tesselateWaveShader->ssrParameters.ssrWorldLength, 0.1f, 0.001f, 500);
	ImGui::DragInt("Steps", &tesselateWaveShader->ssrParameters.ssrMaxSteps, 1, 0, 3000);
	ImGui::DragFloat("Resolution", &tesselateWaveShader->ssrParameters.resolution, 0.0001, 0.0001, 1);
	ImGui::DragFloat("Thickness In Units", &tesselateWaveShader->ssrParameters.thickness, 0.1f, 0.01f, 150.0f);

	ImGui::End();
}

void Scene::buoyancyParametersGuiWindow()
{
	ImGui::Begin("Buoyancy Parameters");

	ImGui::Checkbox("Debug Draw Forces", &buoyancyComputeShader->debugVisualizeBuoyantForces);

	float mass = shipHullRigidBody->getMass();
	ImGui::DragFloat("Body Mass", &mass, 1, 0, 200);
	shipHullRigidBody->setMassProps(mass, shipHullRigidBody->getLocalInertia());

	ImGui::DragFloat("Fluid Density", &buoyancyComputeShader->buoyancyParameters.fluidDensity, 0.001, 0.01, 1);
	ImGui::DragFloat("Column Surface", &buoyancyComputeShader->buoyancyParameters.columnSurface, 0.1, 0, 50);
	ImGui::DragFloat("Column Volume Max", &buoyancyComputeShader->buoyancyParameters.maxColumnVolume, 0.1, 0, 50);
	float damping = shipHullRigidBody->getLinearDamping();
	float angularDamping = shipHullRigidBody->getAngularDamping();
	ImGui::DragFloat("Linear Damping", &damping, 0.01, 0, 1);
	ImGui::DragFloat("Angular Damping", &angularDamping, 0.01, 0, 1);
	shipHullRigidBody->setDamping(damping, angularDamping);
	ImGui::Text("Gradient Descent Wave Height Prediction");
	ImGui::DragFloat("GD: Eps", &buoyancyComputeShader->gradientDescentParameters.eps, 0.0001f, 0.0001f, 0.4f);
	ImGui::DragFloat("GD: LearningRate", &buoyancyComputeShader->gradientDescentParameters.learningRate, 0.01f, 0.1f, 1);
	ImGui::DragInt("GD: Iterations", &buoyancyComputeShader->gradientDescentParameters.iterations, 1, 0, 300);
	ImGui::DragFloat("GD: OffsetAlongAxis", &buoyancyComputeShader->gradientDescentParameters.offsetAlongAxis, 0.1f, 0.1f, 2);

	ImGui::End();
}

void Scene::waveParametersGuiWindow()
{
	ImGui::Begin("Wave Parameters");
	ImGui::Checkbox("Is Rendering Waves", &isRenderingWaves);
	for (size_t i = 0; i < 3; i++)
	{
		std::string waveNum = "Wave" + std::to_string(i) + "Properties";
		ImGui::Text(waveNum.c_str());
		ImGui::DragFloat(("Wave Steepness" + std::to_string(i)).c_str(), &waveParams[i].steepness, 0.01, 0.0f, 1.0f);
		ImGui::DragFloat(("Wavelength" + std::to_string(i)).c_str(), &waveParams[i].wavelength, 1.0f, 0.01, 100.0f);
		ImGui::DragFloat(("Wave Speed" + std::to_string(i)).c_str(), &waveParams[i].speed, 0.01f, 0, 5);
		ImGui::DragFloat2(("Wave Dir XZ" + std::to_string(i)).c_str(), waveParams[i].XZdir, 0.01f, 0, 5);
	}
	ImGui::End();
}

void Scene::postProcessingGuiWindow()
{
	ImGui::Begin("Post Processing Effects");

	ImGui::Checkbox("Render Scene To Texture", &this->isRenderSceneToTexture);

	ImGui::Checkbox("Enable Post Processing", &this->isPostProcessing);

	ImGui::BeginGroup();
	ImGui::Checkbox("Enable Bloom", &enableBloom);
	ImGui::DragFloat("Bloom Blur IR", &bloomBlurMask.vInnerRadius, 0.01, 0.025, 0.9);
	//float reaminder = (1 - edgeBlurMask.vInnerRadius) - 0.2;
	ImGui::DragFloat("Bloom Blur OR", &bloomBlurMask.vOuterRadius, 0.01, 0, 1);
	ImGui::DragFloat("Bloom Blur Power", &bloomBlurMask.vPower, 0.5, 1, 15);
	ImGui::EndGroup();

	ImGui::BeginGroup();
	ImGui::BeginGroup();
	ImGui::Checkbox("Enable Blur", &enableBlur);
	ImGui::DragFloat("Blur Inner Radius", &edgeBlurMask.vInnerRadius, 0.01, 0.025, 0.9);
	//float reaminder = (1 - edgeBlurMask.vInnerRadius) - 0.2;
	ImGui::DragFloat("Blur Outer Radius ", &edgeBlurMask.vOuterRadius, 0.01, 0, 1);
	ImGui::EndGroup();

	ImGui::BeginGroup();
	ImGui::Checkbox("Enable Magnify", &enableMagnify);
	ImGui::DragFloat("Magnify Inner Radius", &magnifyMask.vInnerRadius, 0.05, 0.05, 1.0);
	ImGui::DragFloat("Magnify Outer Radius ", &magnifyMask.vOuterRadius, 0.05, magnifyMask.vInnerRadius, 1.0);
	ImGui::EndGroup();

	ImGui::Checkbox("Enable Water Dsitortion", &enableWaterDistortion);

	ImGui::EndGroup();

	ImGui::Text("Underwatter PostProcessing Parameters");
	auto color = new float[3];
	color[0] = distortionBuffer.colorOverlay.x;
	color[1] = distortionBuffer.colorOverlay.y;
	color[2] = distortionBuffer.colorOverlay.z;

	ImGui::ColorEdit3("Underwater Color", color);
	//ImGui::ColorPicker3("Underwater Color",color);

	distortionBuffer.colorOverlay.x = color[0];
	distortionBuffer.colorOverlay.y = color[1];
	distortionBuffer.colorOverlay.z = color[2];

	ImGui::DragFloat("OffsetX", &distortionBuffer.offsetX, 0.0001f, 0, 1);
	ImGui::DragFloat("OffsetY", &distortionBuffer.offsetY, 0.0001f, 0, 1);
	ImGui::DragFloat("SinXFrequency", &distortionBuffer.sinXFrequency);
	ImGui::DragFloat("SinYFrequency", &distortionBuffer.sinYFrequency);

	//ImGui::DragFloat("Power", &edgeBlurMask.vPower, 0.05, 0.05, 1.0);

	ImGui::End();
}

void Scene::mainGuiWindow()
{
	ImGui::Text("FPS: %.2f", timer->getFPS());
	ImGui::Checkbox("Wireframe mode", &wireframeToggle);
	ImGui::Text("Press E to raise camera \nto see the plane being rendered");
	ImGui::Checkbox("Demo Window", &demoWindow);
	if (demoWindow)
	{
		ImGui::ShowDemoWindow();
	}
	ImGui::Text("MISC");
	ImGui::Checkbox("Debug Visualize Shadow Maps", &tesselateWaveShader->debugVisalizeShadowMaps);
	ImGui::DragFloat("Displacement Value", &displacementValue, 0.001, 0.000, 0.1);
}

void Scene::destructableComponentEffectGuiWindow()
{
	ImGui::Begin("Destructable Components");

	ImGui::SliderFloat("Rainbow Power", &insideOutsideShaderInstance->rainbowColors.power, 1, 25);
	ImGui::InputFloat("Normal Amplitude", &insideOutsideShaderInstance->rainbowColors.noiseAmplitude, 1, 25);
	ImGui::InputFloat("Norma Frequency", &insideOutsideShaderInstance->rainbowColors.noiseFrequency, 50, 1500);
	ImGui::InputFloat("Peturbation", &insideOutsideShaderInstance->rainbowColors.normalStrength, 0, 10);
	ImGui::End();
}

void Scene::renderDebugSamplesAcrossRay(XMMATRIX view, XMMATRIX projection)
{
	XMVECTOR raySample;
	XMVECTOR Origin = XMLoadFloat3(&Ray.Origin);
	XMVECTOR Direction = XMLoadFloat3(&Ray.Direction);
	Direction = XMVector3Normalize(Direction);

	float sampleIncrement = (Ray.tMax - Ray.tMin) / this->raySamples;

	for (int i = 1; i <= this->raySamples; ++i)
	{
		float currentIncrement = sampleIncrement * i;
		XMVECTOR currentDirectionVector = Direction * currentIncrement;
		raySample = XMVectorAdd(Origin, currentDirectionVector);

		debugSphere->transform.setPosition(
			raySample.m128_f32[0],
			raySample.m128_f32[1],
			raySample.m128_f32[2]
		);
		debugSphere->transform.setScale(
			this->debugSphereScale,
			this->debugSphereScale,
			this->debugSphereScale
		);

		setupBaseShaderParamters(defaultShadowShader);
		setupInstanceParameter(defaultShadowShader, debugSphere);
		debugSphere->getMesh()->sendData(getDeviceContext());
		defaultShadowShader->render(getDeviceContext(), debugSphere->getMesh()->getVertexCount());
	}
}

void Scene::renderDebugSphereAt(XMMATRIX view, XMMATRIX projection, std::vector<XMVECTOR>& positions, Material* debugMaterial)
{
	for (XMVECTOR p : positions)
	{
		if (debugMaterial == nullptr)
			debugSphere->setMaterial(materials["Yellow"]);
		else
			debugSphere->setMaterial(debugMaterial);

		debugSphere->transform.setPosition(
			p.m128_f32[0],
			p.m128_f32[1],
			p.m128_f32[2]
		);
		debugSphere->transform.setScale(
			this->debugSphereScale,
			this->debugSphereScale,
			this->debugSphereScale
		);

		setupBaseShaderParamters(defaultShadowShader);
		setupInstanceParameter(defaultShadowShader, debugSphere);
		debugSphere->getMesh()->sendData(getDeviceContext());
		defaultShadowShader->render(getDeviceContext(), debugSphere->getMesh()->getVertexCount());
	}
}

XMVECTOR Scene::getDirectionFromInput()
{
	XMVECTOR direction = XMVectorSet(0, 0, 0, 0);
	if (input->isKeyDown('W'))
	{
		//Orient in ship directoin
		//direction = XMVectorSet(0, 0, 1, 0);
		direction = XMVectorSet(1, 0, 0, 0);
	}

	XMMATRIX sm = this->shipMeshInstance->transform.getTransformMatrix();
	direction = XMVector3TransformNormal(direction, sm);
	direction = XMVector3Normalize(direction);
	return direction;
}

void Scene::applyForceToShip(XMVECTOR direction, float mag)
{
	XMVECTOR force = direction * mag;

	shipHullRigidBody->applyCentralForce(btVector3(XMVectorGetX(force), XMVectorGetY(force), XMVectorGetZ(force)));
}

void Scene::steerShip()
{
	if (input->isKeyDown('A'))
	{
		//shipHullRigidBody->applyTorque(btVector3(0, steeringTorqueAmount, 0));
		shipHullRigidBody->applyTorque(btVector3(0, steeringTorqueAmount, 0));
	}
	if (input->isKeyDown('D'))
	{
		shipHullRigidBody->applyTorque(btVector3(0, -steeringTorqueAmount, 0));
	}
}

void Scene::applyPhysicsTranform(btRigidBody* body, MeshInstance* renderInstance)
{
	// Print the cube's position for debugging
//
	btTransform BTTransform;
	body->getMotionState()->getWorldTransform(BTTransform);
	auto btVectorOrigin = BTTransform.getOrigin();

	btQuaternion btQuaternionRot = body->getWorldTransform().getRotation();
	XMVECTOR rightHandedQuaternion = XMVectorSet(btQuaternionRot.getX(), btQuaternionRot.getY(),
		btQuaternionRot.getZ(), btQuaternionRot.getW());
	XMVECTOR leftHandedQuaternion = XMVectorSet(-btQuaternionRot.getX(), -btQuaternionRot.getY(),
		-btQuaternionRot.getZ(), btQuaternionRot.getW());
	btQuaternion btQuaternionLeft = btQuaternion(
		leftHandedQuaternion.m128_f32[0], leftHandedQuaternion.m128_f32[1], leftHandedQuaternion.m128_f32[2], leftHandedQuaternion.m128_f32[3]);
	auto leftAngle = btQuaternionRot.getAngle();
	auto leftAxis = btQuaternionRot.getAxis();
	auto rightAngle = btQuaternionLeft.getAngle();
	auto rightAxis = btQuaternionLeft.getAxis();
	//XMMATRIX rotMat = XMMatrixRotationQuaternion(rightHandedQuaternion);
	renderInstance->transform.setComposeRotationFromQuaternions(true);
	renderInstance->transform.setQuaternion(leftHandedQuaternion.m128_f32[0], leftHandedQuaternion.m128_f32[1],
		leftHandedQuaternion.m128_f32[2], leftHandedQuaternion.m128_f32[3]);
	renderInstance->transform.setPosition(btVectorOrigin.getX(), btVectorOrigin.getY(), btVectorOrigin.getZ());
}

void Scene::visualizePhysicsOfDestructedComponents()
{
	for each (auto physicsMeshInstancePair in destructablePhysicsComponents)
	{
		applyPhysicsTranform(physicsMeshInstancePair.first, physicsMeshInstancePair.second);
	}
}

void Scene::renderPointsAboveAndBelow(XMMATRIX view, XMMATRIX projection)
{
	renderDebugSphereAt(view, projection, proceduralDestruction.pAbovePlane, materials["Green"]);
	renderDebugSphereAt(view, projection, proceduralDestruction.pBelowPlane, materials["Red"]);
	renderDebugSphereAt(view, projection, proceduralDestruction.pIntersections, materials["Cyan"]);
}

void Scene::gui()
{
	// Force turn off unnecessary shader stages.
	renderer->getDeviceContext()->GSSetShader(NULL, NULL, 0);
	renderer->getDeviceContext()->HSSetShader(NULL, NULL, 0);
	renderer->getDeviceContext()->DSSetShader(NULL, NULL, 0);

	ImGui::Begin("Debug Mode");
	ImGui::Checkbox("Is Debug Mode", &this->isDebugMode);
	ImGui::Checkbox("Is Physics Paused", &this->isPhysicsPaused);
	ImGui::Checkbox("Camera To Ship", &this->isCameraAttachedToShip);
	ImGui::End();

	shipRaySetup();
	shipCameraSetup();

	if (isDebugMode)
	{
		destructableComponentEffectGuiWindow();

		//auto ter = (Terrain*)meshes["Terrain"].GetMesh();
		//Terrain gui options
		guiTerrainGeneration();

		guiTestingRay();

		// Build UI
		mainGuiWindow();

		screenSpaceReflectionGuiWindow();

		meshInstanceTreeGuiWindow();

		buoyancyParametersGuiWindow();

		fogParameterGuiWindow();

		waveParametersGuiWindow();

		postProcessingGuiWindow();

		lightEditor.appendToImgui();
		lightEditor.applyChangesTo(this->lights);
	}

	// Render UI
	ImGui::Render();
	ImGui_ImplDX11_RenderDrawData(ImGui::GetDrawData());
}

void Scene::shipRaySetup()
{
	//Configure the ship ray's local values
	ImGui::Begin("Terrain Generation");
	ImGui::InputFloat3("Rel Position", &ShipRay.Origin.x, -25, 25);
	ImGui::SliderFloat3("Rel Direction", &ShipRay.Direction.x, -25, 25);
	ImGui::SliderFloat("Range Min", &ShipRay.tMin, 0, 25);
	ImGui::SliderFloat("Range Max", &ShipRay.tMax, ShipRay.tMin, 25);

	//Setup the parent hierarchy and apply to actual ray
	XMMATRIX shipMatrix = shipMeshInstance->transform.getTransformMatrix();
	XMVECTOR pos = XMLoadFloat3(&ShipRay.Origin);
	XMVECTOR dir = XMLoadFloat3(&ShipRay.Direction);

	pos = XMVector3Transform(pos, shipMatrix);
	dir = XMVector3TransformNormal(dir, shipMatrix);

	XMStoreFloat3(&Ray.Origin, pos);
	XMStoreFloat3(&Ray.Direction, dir);
	Ray.tMin = ShipRay.tMin;
	Ray.tMax = ShipRay.tMax;

	ImGui::End();
}

void Scene::shipCameraSetup()
{
	//Configure the ship ray's local values
	ImGui::Begin("Ship Camera");

	if (isDebugMode == false)
	{
		XMVECTOR tempPos = XMLoadFloat3(&camera->getPosition());
		//XMVECTOR tempDir = XMLoadFloat3(&camera->getRotation());
		ImGui::InputFloat3("Rel Position", &shipCameraRelPosition.m128_f32[0], -25, 25);
		ImGui::SliderFloat("Fixed Height", &shipCameraFixedHeight, -25, 25);

		//Assuem looking to the left
		//XMVECTOR dirAtShip = XMVectorSubtract(shipMeshInstance->transform.getPosition(), tempPos);
		XMVECTOR rayOrigin = XMLoadFloat3(&Ray.Origin);
		XMVECTOR rayDirection = XMLoadFloat3(&Ray.Direction);

		XMVECTOR endOfRay = XMVectorAdd(rayOrigin, (rayDirection * Ray.tMax));

		XMVECTOR dirAtShip = XMVectorSubtract(endOfRay, tempPos);
		XMVECTOR tempDir = XMVectorSet(1, 0, 0, 0);

		XMVECTOR shipPos = this->shipMeshInstance->transform.getPosition();
		XMVECTOR toShip = XMVectorSubtract(shipPos, tempPos);
		toShip = XMVector3Normalize(toShip);

		dirAtShip = XMVector3Normalize(dirAtShip);
		ImGui::SliderFloat3("Rel Direction", &dirAtShip.m128_f32[0], -25, 25);

		//Setup the parent hierarchy and apply to actual ray
		XMMATRIX shipMatrix = shipMeshInstance->transform.getTransformMatrix();

		tempPos = XMVector3Transform(shipCameraRelPosition, shipMatrix);

		tempPos.m128_f32[1] = shipCameraFixedHeight;
		tempDir = XMVector3TransformNormal(dirAtShip, shipMatrix);

		float yaw = std::atan2(XMVectorGetX(toShip), XMVectorGetZ(toShip)) * (180.0f / XM_PI);

		ImGui::SliderFloat("Yawj", &yaw, -25, 25);

		if (yaw < 0.0f)        yaw += 360.0f;
		else if (yaw >= 360)   yaw -= 360.0f;

		camera->setPosition(XMVectorGetX(tempPos), XMVectorGetY(tempPos), XMVectorGetZ(tempPos));
		camera->setRotation(0, yaw, 0);
	}
	ImGui::End();
}

void Scene::guiTerrainGeneration()
{
	ImGui::Begin("Terrain Generation");

	ImGui::Checkbox("Simple Terrain", &this->isRenderingSimpleTerrain);
	ImGui::Checkbox("Extracked Peak", &this->isRenderTerrainPeak);
	ImGui::Checkbox("Destructable Peak", &this->isRenderDestructablePeak);
	ImGui::Checkbox("Tesselated Terrain", &this->isRenderingTessellatedTerrain);
	if (ImGui::CollapsingHeader("Detail"))
	{
		ImGui::SliderFloat("Simple Terrain Detail", &destructableTerrainPeaks.simpleTerrainDetail, 5, 300);
		ImGui::SliderFloat("Tess Terrain Detail", &destructableTerrainPeaks.tessTerrainDetail, 5, 300);
		ImGui::SliderFloat("Terrain Size", &destructableTerrainPeaks.terrainSizeXZUnits, 10, 1000);
		terrainTesselationShader;

		if (ImGui::Button("Apply Terrain Detail"))
		{
			applyTerrainDetail();
		}
	}

	if (ImGui::CollapsingHeader("Transform"))
	{
		XMVECTOR tempPos = destructableTerrainPeaks.tessellatedTerrainQuadInstance->transform.getPosition();
		int posRange = 150;
		ImGui::SliderFloat3("Position", &tempPos.m128_f32[0], -posRange, posRange);
		destructableTerrainPeaks.terrainInstance->transform.setPosition(XMVectorGetX(tempPos), XMVectorGetY(tempPos), XMVectorGetZ(tempPos));
		destructableTerrainPeaks.tessellatedTerrainQuadInstance->transform.setPosition(XMVectorGetX(tempPos), XMVectorGetY(tempPos), XMVectorGetZ(tempPos));
	}

	if (ImGui::CollapsingHeader("Noise Properties"))
	{
		if (ImGui::CollapsingHeader("Noise Main FBM"))
		{
			ImGui::SliderFloat("Wave Amplitude", &terrainTesselationShader->fBMParams.amplitude, 0.0f, 50.0f);
			ImGui::SliderFloat("Wave Frequuency", &terrainTesselationShader->fBMParams.frequency, 0.001, 100);
			ImGui::SliderFloat("fBM: Lacunarity", &terrainTesselationShader->fBMParams.lucanarity, 1, 10);
			ImGui::SliderFloat("fBM: Gain", &terrainTesselationShader->fBMParams.gain, 0.0f, 1.0f);
			ImGui::SliderInt("fBM: Octaves", &terrainTesselationShader->fBMParams.octaves, 1, 10);
		}
		if (ImGui::CollapsingHeader("Noise One"))
		{
			ImGui::SliderFloat("Noise1: Amplitude", &terrainTesselationShader->fBMParams.NoiseOne.amplitude, 0.0f, 50.0f);
			ImGui::SliderFloat("Noise1: Frequuency", &terrainTesselationShader->fBMParams.NoiseOne.frequency, 0.001, 100);
		}
		if (ImGui::CollapsingHeader("Noise Two"))
		{
			ImGui::SliderFloat("Noise2: Amplitude", &terrainTesselationShader->fBMParams.NoiseTwo.amplitude, 0.0f, 50.0f);
			ImGui::SliderFloat("Noise2: Frequuency", &terrainTesselationShader->fBMParams.NoiseTwo.frequency, 0.001, 100);
		}
		if (ImGui::CollapsingHeader("Noise Three"))
		{
			ImGui::SliderFloat("Noise3: Amplitude", &terrainTesselationShader->fBMParams.NoiseThree.amplitude, 0.0f, 50.0f);
			ImGui::SliderFloat("Noise3: Frequuency", &terrainTesselationShader->fBMParams.NoiseThree.frequency, 0.001, 100);
		}
		if (ImGui::CollapsingHeader("Noise Four"))
		{
			ImGui::SliderFloat("Noise4: Amplitude", &terrainTesselationShader->fBMParams.NoiseFour.amplitude, 0.0f, 50.0f);
			ImGui::SliderFloat("Noise4: Frequuency", &terrainTesselationShader->fBMParams.NoiseFour.frequency, 0.001, 100);
		}
		if (ImGui::CollapsingHeader("Noise Four"))
		{
			ImGui::SliderFloat("Noise5: Amplitude", &terrainTesselationShader->fBMParams.NoiseFive.amplitude, 0.0f, 50.0f);
			ImGui::SliderFloat("Noise5: Frequuency", &terrainTesselationShader->fBMParams.NoiseFive.frequency, 0.001, 100);
		}
	}

	if (ImGui::CollapsingHeader(" Rendering"))

	{

		ImGui::SliderFloat("UV Density", &terrainTesselationShader->terrainDiplacementNormalData.uvDensity, 1, 15);
		ImGui::SliderFloat("Displacement Strength", &terrainTesselationShader->terrainDiplacementNormalData.displacementStrength,0.1,5);
		ImGui::SliderFloat("EPS", &terrainTesselationShader->terrainDiplacementNormalData.EPS, 0.00001,0.1);
		ImGui::Checkbox("Debug", &terrainTesselationShader->debugVisalizeShadowMaps);



		ImGui::SliderFloat("Relative Height One", &terrainTesselationShader->TerrainParameters.heightOne, 0, 1);
		ImGui::SliderFloat("Relative Height Two", &terrainTesselationShader->TerrainParameters.heightTwo, terrainShader->TerrainParameters.heightOne, 1);
		ImGui::SliderFloat("Relative Height Three", &terrainTesselationShader->TerrainParameters.heightThree, terrainShader->TerrainParameters.heightTwo, 1);

		ImGui::ColorPicker3("Range One Color", &terrainTesselationShader->TerrainParameters.rangeColorOne.x);
		ImGui::ColorPicker3("Range Two Color", &terrainTesselationShader->TerrainParameters.rangeColorTwo.x);
		ImGui::ColorPicker3("Range Three Color", &terrainTesselationShader->TerrainParameters.rangeColorThree.x);

		ImGui::SliderFloat("Region Sample X ", &this->RegionSampleOffsetX, 0.0f, 50.0f);
		ImGui::SliderFloat("Region Sample Y ", &this->RegionSampleOffsetY, 0.0f, 50.0f);
	}

	ImGui::End();
}

void Scene::applyTerrainDetail()
{
	destructableTerrainPeaks.applyTerrainDetail();

}


void Scene::calculateAllRayIntersections()
{
}

void Scene::guiTestingRay()
{
	ImGui::Begin("Testing Ray Control");

	//ImGui::Checkbox("Render", &this->isRenderDebugRay);
	ImGui::Checkbox("Render Plane A and B", &this->isRenderingAboveAndBelowPoints);

	if (ImGui::Button("AllIntersections"))
	{
		calculateAllRayIntersections();
	}

	ImGui::DragFloat("Bottom Plane Height", &this->destructableTerrainPeaks.bottomPlaneY, 0.1);
	ImGui::DragFloat("Water Plane Height", &this->destructableTerrainPeaks.waterPlaneY, 0.1);
	ImGui::DragInt("Peak Target Dimension", &this->destructableTerrainPeaks.peakTargetDim, 5);
	ImGui::DragInt("Destructable Peak Target Dimension", &this->destructibleChunkTargetDim, 5);

	ImGui::Checkbox("IsIntersecting", &this->hasRayHit);
	ImGui::DragFloat("Debug Sphere", &this->debugSphereScale, 0.5);
	ImGui::InputInt("Ray Samplers", &this->raySamples);
	ImGui::DragFloat3("Origin", &this->Ray.Origin.x, 0.5);
	ImGui::DragFloat3("Direction", &this->Ray.Direction.x, 0.01);
	XMVECTOR temp = XMLoadFloat3(&this->Ray.Direction);
	temp = XMVector3Normalize(temp);
	XMStoreFloat3(&this->Ray.Direction, temp);

	ImGui::DragFloat("Range Min", &this->Ray.tMin, 0.5);
	ImGui::DragFloat("Range Max", &this->Ray.tMax, 0.5);

	ImGui::End();
}


void Scene::changeStateOfCamera() { activeCameraMovement = !activeCameraMovement; }

void Scene::renderTesselatedWave(XMMATRIX view, XMMATRIX projection)
{
	auto lProj = cascadedShadowMaps->getOrthoMatrix(0);
	auto lView = cascadedShadowMaps->getViewMatrix(0);

	tessellationQuadInstance->getMesh()->sendData(renderer->getDeviceContext(),
		D3D11_PRIMITIVE_TOPOLOGY_4_CONTROL_POINT_PATCHLIST);
	ID3D11ShaderResourceView* texture = nullptr;

	if (!tessellationQuadInstance->getMaterial()->diffuseTexture.empty())
	{
		texture = textureMgr->getTexture(tessellationQuadInstance->getMaterial()->diffuseTexture);
	}

	waveParams[0].time = appTime;
	waveParams[1].time = appTime;
	waveParams[2].time = appTime;

	MultipleWaveBuffer buff;
	buff.waves[0] = waveParams[0];
	buff.waves[1] = waveParams[1];
	buff.waves[2] = waveParams[2];

	tesselateWaveShader->setWaveParams(
		renderer->getDeviceContext(), buff);

	tesselateWaveShader->setWorldPositionAndCamera(renderer->getDeviceContext(),
		tessellationQuadInstance->transform.getTransformMatrix(),
		camera->getPosition(),
		SCREEN_NEAR, SCREEN_DEPTH
	);

	if (tesselateWaveShader->ssrParameters.useSSR)
	{
		tesselateWaveShader->setSSRColorAndDepthTextures(
			getDeviceContext(),
			colourShaderResourceView,
			depthShaderResourceView
		);
	}

	tesselateWaveShader->setTesselationFactors(
		renderer->getDeviceContext(), tesselationFactors);

	ShadowMappingLights lightsMatriceData{
		cascadedShadowMaps->getViewMatrix(0),
		cascadedShadowMaps->getOrthoMatrix(0),
		cascadedShadowMaps->getViewMatrix(1),
		cascadedShadowMaps->getOrthoMatrix(1),
		cascadedShadowMaps->getViewMatrix(2),
		cascadedShadowMaps->getOrthoMatrix(2),
	};

	tesselateWaveShader->setShaderParameters(renderer->getDeviceContext(),
		view, projection,
		lightsMatriceData,
		lights.data(),
		camera->getPosition(),
		sWidth,
		sHeight
	);
	tesselateWaveShader->setShaderParametersForInstance(
		renderer->getDeviceContext(),
		tessellationQuadInstance->transform.getTransformMatrix(),
		tessellationQuadInstance->getMaterial(),
		texture
	);
	tesselateWaveShader->render(renderer->getDeviceContext(), tessellationQuadInstance->getMesh()->getIndexCount());
}

void Scene::setupBaseShaderParamters(DefaultShader* baseShader)
{
	baseShader->setShaderParamsNew(
		this->getDeviceContext(),
		this->camera,
		renderer->getProjectionMatrix(),
		cascadedShadowMaps,
		lights.data(),
		fogParameters,
		sWidth,
		sHeight);
}

void Scene::setupInstanceParameter(DefaultShader* baseShader, MeshInstance* instance)
{
	instance->getMesh()->sendData(renderer->getDeviceContext());
	ID3D11ShaderResourceView* texture = nullptr;

	if (!instance->getMaterial()->diffuseTexture.empty())
	{
		texture = textureMgr->getTexture(instance->getMaterial()->diffuseTexture);
	}

	baseShader->setShaderParametersForInstance(renderer->getDeviceContext(), instance->transform.getTransformMatrix(),
		instance->getMaterial(), texture);
}

void Scene::setupFullSreenPostProcessing(TextureShader* textureShader, RenderTexture* renderTexture)
{
	XMMATRIX orthoMatrix = renderer->getOrthoMatrix(); // ortho matrix for 2D rendering
	XMMATRIX orthoViewMatrix = camera->getOrthoViewMatrix(); // Default camera position for orthographic rendering
	textureShader->setMatrices(renderer->getDeviceContext(), renderer->getWorldMatrix(), orthoViewMatrix, orthoMatrix);
	textureShader->setTexture(renderer->getDeviceContext(), renderTexture->getShaderResourceView());
}

void Scene::renderOnFullScreenOrtho(TextureShader* textureShader, RenderTexture* renderTexture)
{
	setupFullSreenPostProcessing(textureShader, renderTexture);
	fullScreenOrthoMesh->sendData(renderer->getDeviceContext());
	textureShader->render(renderer->getDeviceContext(), fullScreenOrthoMesh->getIndexCount());
}