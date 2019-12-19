//repo - Handles users github public projects
use crate::assets::Asset;
#[allow(non_snake_case)]

//TODO: Implement Date Parse

pub struct Repo { Name: String, Tag: String, URL: String, API_URL: String, Description: String, Language: String, Creation_Date: String, Last_Update: String, Last_Push: String}

impl Repo
{
    pub fn get_name(&self) -> &String { &self.Name }
    pub fn get_tag(&self) -> &String { &self.Tag}
    pub fn get_url(&self) -> &String { &self.URL}
    pub fn get_api_url(&self) -> &String { &self.API_URL }
    pub fn get_description(&self) -> &String { &self.Description}
    pub fn get_language(&self) -> &String { &self.Language }
    pub fn get_creation_date(&self) -> &String { &self.Creation_Date }
    pub fn get_last_update(&self) -> &String { &self.Last_Update}
    pub fn get_last_push(&self) -> &String { &self.Last_Push }

    pub(crate) fn AnyUnassigned(arr: [String; 9]) -> bool
    {
        //https://www.programming-idioms.org/idiom/110/check-if-string-is-blank/1761/rust

        //Loop through the array from 0 to 8 (9 Elements, n-1 = 8)
        for x in  0..8 { if arr[x].trim().is_empty() == true { return false } }

        //Return true as block in loop hasn't been called
        return true
    }

    //The "fake" constructor needs an Array of "String" in size of "8"
    pub(in crate::release) fn new(Result: [String; 9]) -> Self
    {
        Self
        {
            /*
                Array should be 9 in length
                [0] Name of project -> "name":
                [1] Full project name -> "full_name":
                [2] Link (URL) -> "html_url"
                [3] API URL -> "url"
                [4] Project description -> "description"
                [5] Projects language -> "language"
                [6] Creation date -> "created_at"
                [7] Latest update -> "updated_at"
                [8] Latest push -> "pushed_at"
            */

            Name: Result[0].clone(),
            Tag: Result[1].clone(),
            URL: Result[2].clone(),
            API_URL: Result[3].clone(),
            Description: Result[4].clone(),
            Language: Result[5].clone(),
            Creation_Date: Result[6].clone(),
            Last_Update: Result[7].clone(),
            Last_Push: Result[8].clone()
        }
    }
}