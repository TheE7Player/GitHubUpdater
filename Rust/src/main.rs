use crate::assets::Asset;
use crate::repo::Repo;
use crate::release::Release;

mod assets;
mod repo;
mod release;

#[allow(non_snake_case)]

fn main()
{
    let version = "0.1";

    println!("RUNNING GitHubUpdater (RUST) version: {}", version);

    Test_Asset();
    Test_Repo();
}

fn Test_Asset()
{
     println!(" == TESTING Test_Asset() ==");
    //Have to include "f64" at end of number
    let test_obj = Asset::new("Ya boi".to_string(), "James".to_string(), 2951456f64);

    println!("{}", test_obj.get_item_size(assets::DisplaySize::Megabytes));
    println!(" == TESTING Test_Asset() ==");
}

fn Test_Repo()
{
    println!(" == TESTING Test_Repo ==");
    //let test_obj = Repo::new(["James".to_string(), "James".to_string(), "James".to_string(),"James".to_string(), "James".to_string(), "James".to_string(), "false".to_string()]);

    //println!("{}", test_obj.get_name());
    //println!("{}", test_obj.get_parsecorrectly());

    println!(" == TESTING Test_Repo ==");
}
