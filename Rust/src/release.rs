//repo - Handles users github public projects
use crate::assets::Asset;
#[allow(non_snake_case)]

//TODO: Implement Date Parse

pub struct Release { Name: String, Tag: String, URL: String, Info: String, Created: String, Published: String, isPreRelease: bool, parseCorrectly: bool, Assets: Vec<Asset>}

impl Release
{
    pub fn get_name(&self) -> &String { &self.Name }
    pub fn get_tag(&self) -> &String { &self.Tag}
    pub fn get_url(&self) -> &String { &self.URL}
    pub fn get_info(&self) -> &String { &self.Info }
    pub fn get_created_date(&self) -> &String { &self.Created}
    pub fn get_published_date(&self) -> &String { &self.Published }
    pub fn get_isprerelease(&self) -> &bool { &self.isPreRelease }
    pub fn get_parsecorrectly(&self) -> &bool { &self.parseCorrectly}
    pub fn get_assets(&self) -> &Vec<Asset> { &self.Assets }

    pub(crate) fn AnyUnassigned(arr: [String; 7]) -> bool
    {
        //https://www.programming-idioms.org/idiom/110/check-if-string-is-blank/1761/rust

        //Loop through the array from 0 to 6 (7 Elements, n-1 = 6)
        for x in  0..6 { if arr[x].trim().is_empty() == true { return false } }

        //Return true as block in loop hasn't been called
        return true
    }

    //The "fake" constructor needs an Array of "String" in size of "7"
    pub(crate) fn new(Result: [String; 7]) -> Self
    {
        Self
        {
            /*
                Array should be 7/8 in length
                Result[0] = Name ("name")
                Result[1] = Tag ("tag_name")
                Result[2] = URL ("html_url")
                Result[3] = INFO ("body")
                Result[4] = Create Date ("created_at")
                Result[5] = Published Date ("published_at")
                Result[6] = PreRelease ? true/false ("prerelease")
            */

            Name: Result[0].clone(),
            Tag: Result[1].clone(),
            Info: Result[2].clone(),
            URL: Result[3].clone(),
            Created: Result[4].clone(),
            Published: Result[5].clone(),

            //Return true if Result[6] is equal to "true" or "True", any else - false.
            isPreRelease: match Result[6].as_str() { "true" => {true} "True" => {true} _ => {false}},
            parseCorrectly: Repo::AnyUnassigned(Result),
            Assets: Vec::new()
        }
    }
}