$site_script = @'
{
    "$schema": "schema.json", 
    "actions": [
    {
            "verb": "triggerFlow",
            "url": "https://prod-22.northeurope.logic.azure.com:443/workflows/baa8d49561d643bf8a637eb48a2dbb51/triggers/manual/paths/invoke?api-version=2016-10-01&sp=%2Ftriggers%2Fmanual%2Frun&sv=1.0&sig=2uYKaQdWZUbcqRz-ZlfjyWzNjK_p4-ddr6O2ZNpG-Xk",
            "name": "Apply PnP Template",
            "parameters": {
                "event":"",
                "product":""
            }
    }
    ],
    "bindata": {},
    "version": 1
}
'@

Add-SPOSiteScript -Title "Trigger Drone template provisioning" -Content $site_script -Description "Applies drone communication site template via azure web job"